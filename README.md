# Run-As-Service
**Install** any application as a **Windows Service** simply **editing a json file**

#### Dependencies:
- .Net Core 3.1
- Topshelf 4.3.0
- NLog 4.7.11

## How to use:
- Specify in the `appsettings.json` file your executable path, command line arguments and Windows Service parameter
- Open a `Command Prompt` as Administrator and run `.\RunAsService.exe install` to install your application as Windows Service
- That's all!

## Todo: 
- add the startup options (automatic|automatic with delay|manual)
