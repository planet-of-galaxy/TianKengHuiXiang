using UnityEngine;
using QFramework;

public class BulletController : MonoBehaviour, IController
{
    private float _lifetime = 5f;
    private bool _isDestroyed = false;

    void Start()
    {
        Destroy(gameObject, _lifetime);
    }

    public IArchitecture GetArchitecture()
    {
        return GameApp.Interface;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_isDestroyed)
            return;
        
        _isDestroyed = true;
        Destroy(gameObject);
    }
}
