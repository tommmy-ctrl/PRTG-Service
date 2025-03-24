# PRTG Service - Documentation

## Description
The **PRTG Service** is a Windows Service that periodically retrieves sensor data from multiple PRTG servers and saves it in JSON files.

---

## Prerequisites
- **Operating System**: Windows 10/11 or server editions with .NET support
- **.NET Version**: .NET Framework 4.7.2
- **Administrator Rights**: Required for installation and uninstallation
- **PRTG Server**: Access to the PRTG API (Username/Password or API Token)

---

## Installation Guide
**Download the latest version under Releases**
### Steps:
1. **Prepare Files**:
   Unzip the `PRTGService.zip`

2. **Run Installation Script**:
   - Run the `install.bat` file as an Administrator.
   - The script will:
     - Create necessary directories (`C:\ProgramData\PRTGSensorStatus\Program` and `C:\ProgramData\PRTGSensorStatus\Logs`).
     - Copy the program files.
     - Start the **Configurator** to create the `appsettings.json`.
     - Install and automatically start the `PRTGService` service.

3. **Configure the Service**:
   - The **Configurator** will guide you through configuring the PRTG servers.
   - You will need to input server details like IP address, protocol (http/https), API Token or Username/Password, and the query interval.
   - The `appsettings.json` configuration file will be saved in the directory `C:\ProgramData\PRTGSensorStatus\Appsettings\`.

---

## Uninstallation Guide
### Steps:
1. **Stop and Remove the Service**:
   - Run the `uninstall.bat` file as an Administrator.
   - The script will:
     - Stop the `PRTGService` service if it is running.
     - Remove the service from the Windows Services management.
     - Delete the installation directory (`C:\ProgramData\PRTGSensorStatus\Program`) and associated logs.

---

## Configuration
### Files:
- **`appsettings.json`**:  
  - Contains the configuration for the PRTG servers to be queried.
  - Structure:
    ```json
    {
      "Servers": [
        {
          "ServerIP": "yourcoreserver.com",
          "UseAPIToken": true,
          "APIToken": "YourAPIKey",
          "Username": "N/A",
          "Password": "N/A",
          "Protocol": "https",
          "UseAlternatePort": false,
          "Port": "",
          "RefreshInterval": "30"
        }
      ]
    }
    ```
  - **Location**: `C:\ProgramData\PRTGSensorStatus\Appsettings\`

- **Generation**:
  - The `appsettings.json` is created by **Configurator.exe**. Alternatively, it can be manually edited.

---

## Functionality
1. **Sensor Data Fetching**:
   - The service periodically fetches sensor data from the PRTG servers defined in the `appsettings.json`.
   - The fetch interval is configured individually for each server.

2. **Storing Sensor Data**:
   - Sensor data is saved in JSON format.
   - The file names contain the server IP address and a timestamp, e.g.:
     ```
     SensorData_monitoring1.website.de_20241209_120000.json
     ```
   - Storage Location: `C:\ProgramData\PRTGSensorStatus\SensorUpdates`

3. **File Management**:
   - Only a specified number (twice the number of servers) of sensor files are stored per server. Older files are automatically deleted.

---

## Log Files
- **Location**:
  - `C:\ProgramData\PRTGSensorStatus\Logs\ServiceLog.txt`
- **Content**:
  - Information about the service status:
    - Successful API queries and data storage.
    - Errors when fetching data.
    - Automatic management of stored files.

---

## Troubleshooting
### Common Issues:
1. **Service Does Not Start**:  
   - Ensure the `install.bat` was run as Administrator.
   - Check the logs in the `C:\ProgramData\PRTGSensorStatus\Logs` folder.

2. **Missing Sensor Data**:  
   - Check the API URL and the configuration (`appsettings.json`).
   - Make sure the API token or access credentials are correct.

3. **API Errors or Warnings**:  
   - The PRTG API can be filter-dependent. The script has been adjusted to always capture all relevant data.

4. **Exceeded Maximum File Count**:  
   - Older files are automatically deleted. If this does not happen, check the logs.

---

## Technical Details
- **Service Entries**:
  - The service is registered under the name `PRTGService` in the Windows Services management.
  - Automatically starts after installation.

- **Configurator**:
  - **Configurator.exe** creates and manages the `appsettings.json`.

(C) Tom Kölsch @ CNAG
