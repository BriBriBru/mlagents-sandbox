using UnityEngine;

public class ParkingPlace : MonoBehaviour
{
    public short numBeaconInPlace = 0;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("AgentCarBeacon"))
        {
            numBeaconInPlace++;
        }
    }

    void OnTriggerExit(Collider other)
    {

        if (other.CompareTag("AgentCarBeacon"))
        {
            numBeaconInPlace--;
        }
    }
}
