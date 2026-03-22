using UnityEngine;
using QFramework;

public class MonsterController : MonoBehaviour, IController
{
    private int _health = 100;
    private bool _isDestroyed = false;

    public IArchitecture GetArchitecture()
    {
        return GameApp.Interface;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (_isDestroyed)
            return;

        if (collision.gameObject.GetComponent<BulletController>() != null)
        {
            _health -= 20;
            if (_health <= 0)
            {
                this.SendCommand(new KilledCommand());
                Destroy(gameObject);
            }
        }
    }
}
