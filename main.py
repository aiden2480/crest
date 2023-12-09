"""
A program to post Scouts Terrain updates into a Jandi channel at the same
time every week. Multiple profiles from separate units are supported,
simply add them to the profiles array. See README.md for more information
on setup and usage.

The program fetches data from the approvals queue and parses it before
sending it via the Jandi webhook. Extra information about each approval is
fetched and included in the message, for example if the approval request
is a Special Interest Area (SIA), the project name is included. If the
request is an Outdoor Adventure Skill (OAS), the branch is included.
Relevant emojis are also added, as is necessary.

Author: GitHub @aiden2480
"""

from terrain_approvals import TerrainApprovalsTask

import sched
import time

def main():
    schedule = sched.scheduler(time.time, time.sleep)
    tasks = []

    tasks.extend(TerrainApprovalsTask.get_tasks())

    for task in tasks:
        task.create_next_task(schedule, setup=True)

    schedule.run()


if __name__ == "__main__":
    main()
