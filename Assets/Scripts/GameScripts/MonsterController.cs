using UnityEngine;
using QFramework;

public class MonsterController : MonoBehaviour, IController
{
    public GameObject ArmLeft;
    public GameObject ArmRight;
    public GameObject Head;

    private int _health = 100;
    private bool _isDestroyed = false;
    private bool _armDestroyed = false;
    private bool _headDestroyed = false;

    public IArchitecture GetArchitecture()
    {
        return GameApp.Interface;
    }


    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("" + collision.gameObject.name);
        if (_isDestroyed)
            return;

        if (collision.gameObject.GetComponent<BulletController>() != null)
        {
            _health -= 10;

            if (_health <= 50 && !_armDestroyed)
            {
                _armDestroyed = true;
                if (Random.value > 0.5f && ArmLeft != null)
                    Destroy(ArmLeft);
                else if (ArmRight != null)
                    Destroy(ArmRight);
            }

            if (_health <= 10 && !_headDestroyed && Head != null)
            {
                _headDestroyed = true;
                Destroy(Head);
            }

            if (_health <= 0)
            {
                this.SendCommand(new KilledCommand());
                Destroy(gameObject);
            }
        }
    }
}
