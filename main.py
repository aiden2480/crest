"""
A program to post Scouts Terrain updates into a Jandi channel at the same
time every week. See README.md for more information on setup and usage.

The program fetches data from the approvals queue and parses it before
sending it via the Jandi webhook. Extra information about each approval is
fetched and included in the message, for example if the approval request
is a Special Interest Area (SIA), the project name is included. If the
request is an Outdoor Adventure Skill (OAS), the branch is included.
Relevant emojis are also added, as is necessary. 
"""

import json
import sched
import requests

import datetime as dt
import time as timemod

from functools import lru_cache


# Unpack constants from JSON file
with open("credentials.json") as fp:
    raw = json.loads(fp.read())

    LOOKBACK_DAYS = raw["lookback_days"]
    CRON_WEEKDAY = raw["cron_weekday"]
    CRON_TIMESTAMP = raw["cron_timestamp"]
    TERRAIN_USERNAME = raw["terrain_username"]
    TERRAIN_PASSWORD = raw["terrain_password"]
    JANDI_WEBHOOK = raw["jandi_webhook"]

# Main function
def main():
    """
        Runs the main algorithm to fetch and parse Terrain data, before posting to Jandi
    """

    print("Running main function")

    sess = generate_session()
    unit = get_units(sess)[0] # Some users may be in multiple units, this gets the first unit.
                              # You may want to hardcode this value if it suits your needs

    # Get pending/recent and filter out old approvals
    raw_pending = get_pending_approvals(sess, unit)
    raw_recent = get_finalised_approvals(sess, unit)
    raw_recent = filter(lambda i: calculate_date_delta(i["submission"]["date"]) <= LOOKBACK_DAYS, raw_recent)
    raw_recent = filter(lambda i: i["submission"]["outcome"] == "approved", raw_recent)

    # Sort each of the queues and split
    def sort(item):
        return item["member"]["first_name"] + item["member"]["last_name"] + \
            str(calculate_date_delta(item["submission"]["date"]))
    
    raw_pending = sorted(raw_pending, key=sort)
    raw_recent = sorted(raw_recent, key=sort)
    pending = []
    recent = []

    # Split pending queue
    for index, item in enumerate(raw_pending):
        if index == 0 or item["member"]["id"] != raw_pending[index - 1]["member"]["id"]:
            pending.append([])
        
        pending[-1].append(item)

    # Split approved queue
    for index, item in enumerate(raw_recent):
        if index == 0 or item["member"]["id"] != raw_recent[index - 1]["member"]["id"]:
            recent.append([])
        
        recent[-1].append(item)

    # Pending approvals
    embed = {
        "body": "Pending approval requests",
        "connectColor": "#FAC11B",
        "connectInfo": [],
    }

    for member_events in pending:
        member = member_events[0]["member"]
        connect = {
            "title": member["first_name"] + " " + member["last_name"],
            "description": "",
        }

        for approval in member_events:
            connect["description"] += get_achievement_meta(sess, approval)
            connect["description"] += "\n"
        
        embed["connectInfo"].append(connect)
    
    empty = [{"description": "No pending approvals"}]
    embed["connectInfo"] = embed["connectInfo"] or empty
    send_jandi_webhook(JANDI_WEBHOOK, embed)

    # Recently approved
    embed = {
        "body": f"Approved in the last {LOOKBACK_DAYS} days",
        "connectColor": "#2ECC71",
        "connectInfo": [],
    }

    for member_events in recent:
        member = member_events[0]["member"]
        connect = {
            "title": member["first_name"] + " " + member["last_name"],
            "description": "",
        }

        for approval in member_events:
            connect["description"] += get_achievement_meta(sess, approval)
            connect["description"] += "\n"
        
        embed["connectInfo"].append(connect)
    
    empty = [{"description": "No recent approvals"}]
    embed["connectInfo"] = embed["connectInfo"] or empty
    send_jandi_webhook(JANDI_WEBHOOK, embed)

# Terrain API functions
def get_pending_approvals(sess: requests.Session, unit: str):
    """
        Fetches the approvals that are pending in queue
    """
    url = f"https://achievements.terrain.scouts.com.au/units/{unit}/submissions?status=pending"
    return sess.get(url).json()["results"]

def get_finalised_approvals(sess: requests.Session, unit: str):
    """
        Fetches approvals that have been finalised (both approved or denied)
    """
    url = f"https://achievements.terrain.scouts.com.au/units/{unit}/submissions?status=finalised"
    return sess.get(url).json()["results"]

def get_units(sess: requests.Session) -> list:
    """
        Returns a list of IDs for units
    """
    url = "https://members.terrain.scouts.com.au/profiles"
    data = sess.get(url).json()

    return [prof["unit"]["id"] for prof in data["profiles"]]

def get_achievement_meta(sess: requests.Session, event: dict) -> str:
    """
        Fetches more information about an achievement from the
        Terrain API, prepending emojis as is necessary 
    """

    achievements = {
        "intro_scouting" : "⚜️ Introduction to Scouting",
        "intro_section" : "🗣️ Introduction to Section",
        "course_reflection" : "📚 Personal Development Course",
        "adventurous_journey" : "🚀 Adventurous Journey",
        "personal_reflection" : "📐 Personal Reflection",
        "peak_award" : "⭐ Peak Award",
    }

    oas_emoji = {
        "alpine" : "❄️",
        "aquatics" : "🏊",
        "boating" : "⛵",
        "bushcraft" : "🏞️",
        "bushwalking" : "🥾",
        "camping" : "⛺",
        "cycling" : "🚲" ,
        "paddling" : "🛶",
        "vertical" : "🧗",
    }

    sia = {
        "sia_adventure_sport" : "🏈 Adventure & Sport",
        "sia_art_literature" : "🎭 Arts & Literature",
        "sia_better_world" : "🌏 Creating a Better World",
        "sia_environment" : "♻️ Environment",
        "sia_growth_development" : "🌱 Growth & Development",
        "sia_stem_innovation" : "🔎 STEM & Innovation",
    }

    expand = lambda item: item.replace("_", " ").capitalize()
    result = achievements.get(event["achievement"]["type"])

    # Find OAS branch and level
    if not result and event["achievement"]["type"] == "outdoor_adventure_skill":
        past = get_member_achievements(sess, event["member"]["id"])
        
        for item in past:
            if item["id"] == event["achievement"]["id"]:
                meta = item["achievement_meta"]
                emoji = oas_emoji.get(meta["stream"])
                branch = meta["branch"].title().replace("-", " ")

                result = f"{emoji} {branch} stage " + str(meta["stage"])

    # Find SIA type and name
    if not result and event["achievement"]["type"] == "special_interest_area":
        past = get_member_achievements(sess, event["member"]["id"])

        for item in past:
            if item["id"] == event["achievement"]["id"]:
                selection = sia.get(item["answers"]["special_interest_area_selection"], "Unknown")
                name = item["answers"].get("project_name", "Unnamed project").strip()
                event_type = event["submission"]["type"]
                
                result = f"{selection} SIA - {name} ({event_type})"

    # Find milestone level
    if not result and event["achievement"]["type"] == "milestone":
        past = get_member_achievements(sess, event["member"]["id"])
        
        for item in past:
            if item["id"] == event["achievement"]["id"]:
                meta = item["achievement_meta"]

                result = "👣 Milestone " + str(meta["stage"])


    return result or expand(event["achievement"]["type"])

@lru_cache(maxsize=None)
def get_member_achievements(sess: requests.Session, member: str) -> list:
    """
        Given a member ID, returns that member's achievements as fetched from Terrain.
        Function results are cached to prevent unnessecary API calls.
    """
    
    url = f"https://achievements.terrain.scouts.com.au/members/{member}/achievements"
    data = sess.get(url).json()["results"]

    return data

# Helper functions
def send_jandi_webhook(url: str, body: dict) -> requests.Response:
    """
        Accepts parameter `body` as json data to send to the JANDI url
    """
    
    headers = {
        "Accept": "application/vnd.tosslab.jandi-v2+json",
        "Content-Type": "application/json",
    }

    return requests.post(url, headers=headers, json=body)

def calculate_date_delta(isoformat: str) -> int:
    """
        Calculates the number of days that have passed since a timestamp
    """
    stamp = dt.datetime.fromisoformat(isoformat).replace(tzinfo=None)
    now = dt.datetime.now().replace(tzinfo=None)

    return (now - stamp).days

def generate_session() -> requests.Session:
    """
        Logs in the requests Session and attaches the authentication header
    """
    sess = requests.Session()

    body = {
        "ClientId": "6v98tbc09aqfvh52fml3usas3c",
        "AuthFlow": "USER_PASSWORD_AUTH",
        "AuthParameters": {
            "USERNAME": TERRAIN_USERNAME,
            "PASSWORD": TERRAIN_PASSWORD,
        }
    }

    headers = {
        "Content-Type": "application/x-amz-json-1.1",
        "X-amz-target": "AWSCognitoIdentityProviderService.InitiateAuth",
    }

    url = "https://cognito-idp.ap-southeast-2.amazonaws.com/"
    resp = sess.post(url, json=body, headers=headers)

    # Ensure credentials are correct
    if resp.status_code == 400:
        raise RuntimeError(resp.json())

    # Attach the auth header & return session
    sess.headers.update({
        "Authorization": resp.json()["AuthenticationResult"]["IdToken"]
    })
    
    return sess

# Schedule functions
def create_task(schedule: sched.scheduler, wait: bool = False):
    """
        Creates a task to run the main function and then
        recursively calls itself to continuously repeat
        the task

        The wait flag is only set if this function has not
        been called recursively and as such the first run
        may be premature so it is skipped
    """

    # Calculate next run
    today = dt.date.today()
    targetday = today + dt.timedelta((CRON_WEEKDAY - today.weekday()) % 7)
    hour, minute = CRON_TIMESTAMP.split(":")

    # Combine date and time into one object
    target = dt.datetime.combine(targetday, dt.time(
        hour=int(hour), minute=int(minute)
    ))

    # If today is a scheduled date and that time has passed
    if target < dt.datetime.now():
        target += dt.timedelta(weeks=1)

    # Schedule next run
    schedule.enterabs(target.timestamp(), 1, create_task, (schedule,))
    print("Scheduling next run for", target)
    
    # The wait flag is only set if this function has not been called
    # recursively and as such the first run may be premature so it
    # is skipped
    if wait:
        return

    # Run main function
    main()


if __name__ == "__main__":
    schedule = sched.scheduler(timemod.time, timemod.sleep)

    # Create task to run main function
    create_task(schedule, wait=True)

    # Run schedule forever
    schedule.run()
