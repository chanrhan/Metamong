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
using Unity.VisualScripting;
using UnityEngine;

public class RelayUtils 
{
    private static int m_MaxConnections = 4;
    private static Allocation serverAllocation = null;
    private static JoinAllocation clientAllocation = null;
    private static string joinCode = "";

    public static async Task CreateRelay(){
        try{
            
            if(serverAllocation == null){
                Debug.Log("Create Relay");
                serverAllocation = await RelayService.Instance.CreateAllocationAsync(m_MaxConnections);

                joinCode = await RelayService.Instance.GetJoinCodeAsync(serverAllocation.AllocationId);

                RelayServerData relayServerData = new RelayServerData(serverAllocation, "dtls");
                CustomNetworkManager.Instance.GetComponent<UnityTransport>().SetRelayServerData(
                    relayServerData
                );
            }else{
                Debug.Log("Relay already created");
            }
            // Set Join Code
            ClientManager.Instance.JoinCode = joinCode;
            UIManager.Instance.SetChannelCode(joinCode);

            CustomNetworkManager.Instance.StartHost();
        }catch(RelayServiceException e){
            Debug.Log(e);
        }

    }

    public static async Task JoinRelay(string joinCode){
        try{
            if(clientAllocation == null){
                Debug.Log("Joining Relay with " + joinCode);
                clientAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

                RelayServerData relayServerData = new RelayServerData(clientAllocation, "dtls");
                CustomNetworkManager.Instance.GetComponent<UnityTransport>().SetRelayServerData(
                    relayServerData
                );
            }else{
                Debug.Log("Relay already created");
            }

            CustomNetworkManager.Instance.StartClient();
        }catch(RelayServiceException e){
            Debug.Log(e);
        }
    }
}
