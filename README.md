# Docker port of Crest

I have no idea what I'm doing. This is on GitHub for three reasons:
- It lets me pull changes from the original to my Docker setup
- It gives me a file backup
- I feel like I'm contributing to the world when I publish even the tiniest little things on GitHub.

**So what's changed in this fork?**

- Changed some boolean values in the main `.csproj` file because Docker didn't like them
- Removed the `.csproj` file's reference to the app icon, because Docker didn't like it
- Changed the demo YAML file so it's just the ScoutEvent crawler (you can change this back)
- Changed the hex code of the colour sent to JANDI for the webhook to the Venturer red
- There's also a folder called "Docker stuff" that you can `docker compose up --build` from if you `cd` into it, and it should work

# :mountain: crest
<div align="center">
    <img src="https://img.shields.io/github/last-commit/aiden2480/crest?color=yellow" alt="Last commit" />
    <img src="https://img.shields.io/github/license/aiden2480/crest" alt="License" />
    <img src="https://img.shields.io/github/languages/code-size/aiden2480/crest" alt="Code size" />
    <img src="https://github.com/aiden2480/crest/actions/workflows/dotnet-ci.yml/badge.svg" alt="Publish & Release workflow status" />
</div>

A program to automate common tasks with managing a scout unit. The program runs any number of configurable extensions preriodically, and as such it can be set up to run for multiple sections or units within one configuration file. Currently, the following extensions have been implemented:

- **Terrain Approvals**: Fetches data from the approvals queue and parses it before sending it via the Jandi webhook. Extra information about each approval is fetched and included in the message, for example if the approval request is a Special Interest Area (SIA), the project name is included. If the request is an Outdoor Adventure Skill (OAS), the branch is included. See examples below for sample output.
- **Scout Event Crawler**: Compatible with [NSW ScoutEvent](https://events.nsw.scouts.com.au), this extension periodically parses the website for any new activities and posts them to a specified Jandi topic.
 
## :file_cabinet: Configuration
Place a file named `config.yaml` next to the application. This file will define the configuration to run the application with. For an example file, see [config.yaml](Crest/config.yaml), for others, check the [example yaml files](Crest.Test/TestFiles). The exact nature of the config file is described in [ApplicationConfiguration](Crest/Integration/ApplicationConfiguration.cs)

```yaml
terrain_approvals:
  enabled: true
  tasks:
  - task_name: Scouts Terrain Approvals
    
    # Username and password for Terrain in the format branch-memberid
    username: nsw-123123
    password: ScoutsTerrainPassword

    # The GUID of the target unit. You must have unit council permissions for said unit. This can be found from opening network requests on Terrain
    unit_id: b3895042-9e1d-42ed-87d1-30b13020859f

    # The number of days to look backwards for recent approvals. See examples below
    lookback_days: 90

    # The incoming webhook url to post to
    jandi_url: https://wh.jandi.com/connect-api/webhook/65436543/63456h634564h6534

    # The cron schedule to run this task on. Make/validate your own at http://cronmaker.com/
    cron_schedule: 0 0 17 ? * TUE

  # This second task will ran in parallel with the first
  - task_name: Vennies Terrain Approvals
    username: nsw-321321
    password: password2
    unit_id: fb0b4b9b-e549-401d-93a8-10da3b89a047
    lookback_days: 60
    jandi_url: https://wh.jandi.com/connect-api/webhook/763576/554g3h25g3425342522
    cron_schedule: 0 0 18 ? * THU

scout_event_crawler:
  tasks:
  - task_name: Scout Event Crawler - All Sections
    cron_schedule: 0 0 12 ? * MON
    jandi_url: https://wh.jandi.com/connect-api/webhook/763576/5435g4354353454j353
    
    # A list of regions from NSW to subscribe to updates from. Below are all possible values
    subscribed_regions:
    - state
    - south_metropolitan
    - sydney_north
    - greater_western_sydney
    - hume
    - south_coast_tablelands
    - swash
```

## :runner: Running
Download and unzip the [latest release](https://github.com/aiden2480/crest/releases/latest) and run the executable. The bash script below will do the same. If you have dotnet 6.0 installed then you can download the smaller framework-dependent file, otherwise the self-contained version must be used. 

```bash
# Check the Releases tab for a more suitable asset
release_asset="crest-ubuntu-self-contained.zip"

tag=$(curl -s "https://api.github.com/repos/aiden2480/crest/releases/latest" | grep -o '"tag_name": ".*"' | cut -d'"' -f4)
if [ -z "$tag" ]; then
    echo "Error: Unable to retrieve latest release tag."
    exit 1
fi

download_url="https://github.com/aiden2480/crest/releases/download/$tag/$release_asset"
echo "Downloading from $download_url"

response=$(curl -sL -w "%{http_code}" -o $release_asset "$download_url")

if [ "$response" != "200" ]; then
    echo "Error: Failed to download release asset. HTTP status code: $response"
    exit 1
fi

echo "Download completed successfully. Extracting..."
unzip -oq $release_asset -d ./
rm $release_asset
echo "Unzipped successfully"

chmod u+x Crest
```

## :camera_flash: Program screenshots
<div align="center">
    <img height="700px" src="https://user-images.githubusercontent.com/19619206/182129371-f943fecb-f86d-4903-a065-c66a6f5b3eda.png" />
    <img height="700px" src="https://user-images.githubusercontent.com/19619206/182129485-9ebe85fc-cb13-4847-85f9-455eae6aed9d.png" />
    <br /><i>Screenshots taken from two different units depicting the pending queue and recently approved achievements</i>
</div>

## :memo: Future features
- [ ] Send emails instead of posting to Jandi topic, or output to any other platform
- [ ] More feedback in console when tasks are run/correct logging
- [ ] Superceeded document notifier to inform when any documents on the scouts page have been updated
- [x] Add compatability with multiple units/profiles at once
