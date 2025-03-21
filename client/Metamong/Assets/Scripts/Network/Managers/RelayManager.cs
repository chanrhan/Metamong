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

public class RelayManager : MonobehaviourSingleton<RelayManager>
{
    [SerializeField]
    private int m_MaxConnections = 4;

    public async Task CreateRelay(){
        try{
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(m_MaxConnections);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("JoinCode: " + joinCode);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            CustomNetworkManager.Instance.GetComponent<UnityTransport>().SetRelayServerData(
                relayServerData
            );

            CustomNetworkManager.Instance.StartHost();
        }catch(RelayServiceException e){
            Debug.Log(e);
        }

    }

    public async Task JoinRelay(string joinCode){
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
