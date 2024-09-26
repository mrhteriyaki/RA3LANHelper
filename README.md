# Command and Conquer Red Alert 3 LAN Helper.

This program fixes a connection issue within LANs for Revora / CNC Online  

Typical error that occurs with same player on the network between Players 1 and 2.  
>*Connections are in progress, or connection problem detected. Please wait for the connection to finish, or kick the player who has the connection problem.  
Connection in progress: 1-2*  


RA3 tries to create a P2P mesh network between each player and is unable to link users within the same internal network.
The destination IP:PORT is determined by the NAT Negotiation server using the source address of the client.
Other CNC games and BFME2 connect directly to the internal IP of the P2P client, RA3 does not do this and attempts to use the public address.
As the ports are dynamically selected they cannot be forwarded, this tool redirects them to a set range of ports that can be forwarded.  

The firewall overide port may have been for this purpose in RA3, however it does not work when set manually.
This program redirects the random UDP port traffic used by NAT Negotiaton to a port that is manually set by this tool.
These ports can be forwarded (with NAT Loopback) to allow internal access between game clients.


### Usage Instructions
1. Download [CNCLocalRelay](https://github.com/mrhteriyaki/RA3LANHelper/releases/download/release/CncLocalRelay.zip) or compile yourself.
2. Run CNC Local Relay GUI.exe
3. Enable redirection to the CNC-Online NAT Test server by clicking enable (Program must be running as admin for this).
4. Choose a starting port or use the random one assigned.
5. Click Start Relay (Windows Firewall warning may occur, make sure to check all network types and click Allow).
6. Run RA3
 

**Port Forwarding Note:**  
UPNP will try to automatically open required ports but can be unreliable depending on the router or may not include NAT Loopback.
You can manually port forward on your router to make it more reliable.
RA3 slowly increments the port number in use, so forward a range of 50 ports from the starting port number set on the relay.
EG: PC1 has a start port 50000 would need UDP Ports 50000-50050 forwarded, PC2 with start port 40000 would need 40000-40050.  
NAT has been successfully tested on a Mikrotik router.  

**Notes & Limitations**
- Dropout / Disconnection handling does not work if you stop the relay and resume it will cause player dropout.
- Co-Op Campaign is supported Relay has been tested and does resolve same issue for campaign co-op.
- The connection attempt between players only occurs for a short period of time when the player joins the match. The game indicates it is retrying to connect to players in the chat but it isn't, connection attempts will stop after the first minute.


**Command Line Version**  
For a command line version of the program that includes console logging information, use the CncLocalRelay.exe
The only parameter required is the starting port range eg: CnCLocalRelay.exe 51000
This may also work with Linux systems but is untested.