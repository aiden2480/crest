# :mountain: crest
A program to post Scouts Terrain updates into a Jandi channel at the same time every week. See below for more information on setup and usage.
The program fetches data from the approvals queue and parses it before sending it via the Jandi webhook. Extra information about each approval is fetched and included in the message, for example if the approval request is a Special Interest Area (SIA), the project name is included. If the request is an Outdoor Adventure Skill (OAS), the branch is included. Relevant emojis are also added, as is necessary. 

<div align="center">
    <img src="https://img.shields.io/github/last-commit/aiden2480/crest?color=yellow" alt="Last commit" />
    <img src="https://img.shields.io/github/license/aiden2480/crest" alt="License" />
    <img src="https://img.shields.io/github/languages/code-size/aiden2480/crest" alt="Code size" />
</div>

## :key: Credentials
To run the program, you will need to rename the example credentials file to `credentials.json` and fill in the values, which are as follows:

| Property           | Value type     | Description                                                     | Example                      |
|--------------------|----------------|-----------------------------------------------------------------|------------------------------|
| `lookback_days`    | Integer        | The number of days of approved requests to display in the embed | `90`                         |
| `cron_weekday`     | Integer        | The weekday on which to evaluate the program. 0-6 for Mon-Sun   | `1`                          |
| `cron_timestamp`   | String         | The timestamp in 24h time to execute, in the format `HH:MM`     | `"17:00"`                    |
| `terrain_username` | String         | Your state, followed by a hyphen and then member number         | `"nsw-0000"`                 |
| `terrain_password` | String         | Password for Scouts Terrain                                     | `"password"`                 |
| `jandi_webhook`    | String         | The Jandi Connect webhook URL to post data to                   | `"https://wh.jandi.com/xxx"` |

## :snake: Running
The program can be run with the following command after editing the configuration file, as demonstrated above. It is designed to be executed on a server and will execute continously. At the specified time, it will log in to the Scouts | Terrain API and fetch both the recently approved requests and the ones still pending. It will then process the data and post it to the specified Jandi topic. 

```bash
$ python main.py
```

## :file_cabinet: Dependencies
The only third-party package used is [`requests`](https://pypi.org/project/requests/), which can be installed, if not already, with the following:

```bash
$ python -m pip install requests
```

## :camera_flash: Program screenshots
<div align="center">
    <img height="700px" src="https://user-images.githubusercontent.com/19619206/182129371-f943fecb-f86d-4903-a065-c66a6f5b3eda.png" />
    <img height="700px" src="https://user-images.githubusercontent.com/19619206/182129485-9ebe85fc-cb13-4847-85f9-455eae6aed9d.png" />
    <br /><i>Screenshots taken from two different units depicting the pending queue and recently approved achievements</i>
</div>

## :memo: Future features
- [ ] Add compatability with multiple units/profiles at once
- [ ] Send emails instead of posting to Jandi topic
- [ ] Improved error handling for incorrect credentials/other login errors
- [ ] Error handle for if `credentials.json` does not exist/is invalid
- [ ] More feedback in console
- [ ] Check username and password are correct before first run
