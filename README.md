# Command and Conquer Red Alert 3 LAN Helper.

This program fixes a connection issue within LANs for Revora / CNC Online

Typical error that occurs with same player on the network between Players 1 and 2.
>*Connections are in progress, or connection problem detected. Please wait for the connection to finish, or kick the player who has the connection problem.
Connection in progress: 1-2*


**Problem:**\
RA3 tries to create a P2P mesh network between each player and is unable when users 
are behind the same NAT / single public IP, the destination IP:PORT is determined by the NAT Negotiation server using the source address of the client. Other CNC games and BFME2 connect directly to the internal IP of the P2P client, RA3 does not do this.
The firewall overide port in RA3 does not work when set manually preventing the usage of normal port forwarding for each game client.

**Solution:**\
This program redirects the random UDP port traffic used by NAT Negotiaton to a port that is manually set and can then be forwarded (with NAT Loopback) to allow internal access between game clients.


### Usage Instructions
1. Download [CNCLocalRelay](https://github.com/mrhteriyaki/RA3LANHelper/releases/download/untagged-b31feadf2762b6db4dd2/CNCLocalRelay.zip) or compile yourself.
2. Run CNC Local Relay GUI.exe
3. Enable redirection to the CNC-Online NAT Test server by clicking enable (Program must be running as admin for this).
4. Choose a starting port or use the random one assigned.
5. Click Start Relay (Windows Firewall warning may occur, make sure to check all network types and click Allow).
6. Play RA3 Online
 

**Port Forwarding Note:**
UPNP will try to automatically open required ports but can be unreliable depending on the router or may not include NAT Loopback.
You can manually port forward on your router to make it more reliable.
RA3 slowly increments the port number in use, so forward a range of 50 ports from the starting port number set on the relay.
EG: PC1 has a start port 50000 would need UDP Ports 50000-50050 forwarded, PC2 with start port 40000 would need 40000-40050.

**Notes & Limitations**
- Dropout / Disconnection handling does not work, stopping relay and resume will cause player dropout.
- Co-Op Campaign is supported Relay has been tested and does resolve same issue for campaign co-op.
- The connection attempt between players only occurs for a short period of time when the player joins the match. The game indicates it is retrying to connect to players in the chat but it isn't, connection attempts will stop after the first minute.


