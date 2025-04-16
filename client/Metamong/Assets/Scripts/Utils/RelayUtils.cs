using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayUtils 
{
    private static int m_MaxConnections = 4;

    public static async Task CreateRelay(){
        try{
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(m_MaxConnections);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Set Join Code
            ClientManager.Instance.JoinCode = joinCode;
            UIManager.Instance.SetChannelCode(joinCode);
            // Debug.Log("JoinCode: " + joinCode);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            CustomNetworkManager.Instance.GetComponent<UnityTransport>().SetRelayServerData(
                relayServerData
            );

            CustomNetworkManager.Instance.StartHost();
        }catch(RelayServiceException e){
            Debug.Log(e);
        }

    }

    public static async Task JoinRelay(string joinCode){
        try{
            Debug.Log("Joining Relay with " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            CustomNetworkManager.Instance.GetComponent<UnityTransport>().SetRelayServerData(
                relayServerData
            );

            CustomNetworkManager.Instance.StartClient();
        }catch(RelayServiceException e){
            Debug.Log(e);
        }
    }
}
