# Command and Conquer Red Alert 3 LAN Helper.

This program fixes the connection 1-2 issue that occurs within LANs clients while using CNC Online / Revora.  

Typical error that occurs with same player on the network between Players 1 and 2.  
>*Connections are in progress, or connection problem detected. Please wait for the connection to finish, or kick the player who has the connection problem.  
Connection in progress: 1-2*  

This problem occurs due to a limitation in most Router NAT services that don't allow connections from within the network to access the public side port.  
It requires a loopback which can be achived with a port forward rule to known port numbers.  
The RA3 process uses a random (dynamic) port which prevents the normal usage of port forwards except DMZ forwarding of all ports.  
This program acts as a proxy that relays the traffic from the random ports to a range of ports starting from a number you can specify in this tool.  

## Instructions:  
1. Download the tool(available from releases on the right).  
2. Run the tool as administrator (Admin Rights are only required for the Host Redirection setting change).  
3. Click Enable on NAT-NEG Redirection **(This will redirect NAT Negotiation traffic to the local machine and persists even if the app is closed)**
4. Select a starting port range number or use the random one provided (The port range must be unique per machine within your network).  
5. On your router add the port forward rule to include a range of 50 ports from the number selected, to your device. Most routers will have a start and end port option so that you can forward a 'range' of ports (Multiple ports).  
  If you do not know how to do this, you can try the UPNP automatic port forwarding, just tick the box in the app to enable it.  
6. Click Start Relay.  
7. If you have any Windows Firewall prompts, click allow as it uses a listening port for inbound network traffic.  
8. Play RA3 Online.  

Image the tool interface:  
![uiimage](Images/Image1.PNG)  

**Port Forwarding Note:**  
UPNP will try to automatically open required ports but can be unreliable depending on the router or may not include NAT Loopback.
You can manually port forward on your router to make it more reliable.
RA3 slowly increments the port number in use, so forward a range of 50 ports from the starting port number set on the relay.
EG: PC1 has a start port 50000 would need UDP Ports 50000-50050 forwarded, PC2 with start port 40000 would need 40000-40050.  
This has been successfully tested on a Mikrotik router with manual port forwards.  

**Notes & Limitations**
- Dropout / Disconnection handling does not work if you stop the relay and resume it will cause player dropout.
- Co-Op Campaign is supported Relay has been tested and does resolve same issue for campaign co-op.
- The connection attempt between players only occurs for a short period of time when the player joins the match. The game indicates it is retrying to connect to players in the chat but it isn't, connection attempts will stop after the first minute.


**Command Line Version**  
For a command line version of the program that includes console logging information, use the CncLocalRelay.exe
The only parameter required is the starting port range eg: CnCLocalRelay.exe 51000
This may also work with Linux systems but is untested.

**Linux Support Notes**
Not tested however is built without any windows dependencies.
