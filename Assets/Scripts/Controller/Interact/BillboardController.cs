using QFramework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BillboardController : MonoBehaviour, IController
{
    [SerializeField] private Image _key;
    [SerializeField] private Sprite _focusSprite;
    [SerializeField, Range(0f, 1f)] private float _showKeyMaxDot = 0.98f;
    [SerializeField] private float _maxFocusDistance = 5f;

    public UnityEvent OnBillboardFocusOn = new();
    public UnityEvent OnBillboardFocusOff = new();

    private bool _isFocused;
    private Sprite _initialSprite;

    private void Awake()
    {
        if (_key != null)
        {
            _initialSprite = _key.sprite;
        }
    }

    private void OnEnable()
    {
        // 启用时若已直接满足聚焦条件，则立即触发聚焦
        if (IsFocused())
        {
            SetFocused(true);
        }
    }

    private void OnDisable()
    {
        // 关闭时若处于聚焦状态则触发失焦
        if (_isFocused)
        {
            SetFocused(false);
        }

        // 无论是否聚焦，都还原 Sprite，保证关闭后参数一致
        RestoreSprite();
    }

    private void Update()
    {
        if (_key == null)
        {
            return;
        }

        bool focused = IsFocused();
        if (focused != _isFocused)
        {
            SetFocused(focused);
        }
    }

    /// <summary>判断物体是否在当前玩家的聚焦距离内，并大致处于主相机正前方。</summary>
    private bool IsFocused()
    {
        if (_key == null || Camera.main == null)
        {
            return false;
        }

        Camera mainCamera = Camera.main;
        var controllingRole = this.GetSystem<IRoleInstanceSystem>().ControllingRole;
        if (controllingRole == null)
        {
            return false;
        }

        // 距离条件：使用当前玩家到物体的距离。
        float distance = Vector3.Distance(controllingRole.transform.position, transform.position);
        if (distance > _maxFocusDistance)
        {
            return false;
        }

        // 角度条件：使用相机指向物体的方向，与玩家距离计算独立。
        // 点乘 = cos(摄像机朝向 与 指向物体的夹角)，角度越小点乘越大，
        // 当夹角小于指定最大角度(即点乘大于阈值)时视为聚焦
        Vector3 toSelfNormalized = (transform.position - mainCamera.transform.position).normalized;
        float dot = Vector3.Dot(mainCamera.transform.forward, toSelfNormalized);
        return dot > _showKeyMaxDot;
    }

    private void SetFocused(bool focused)
    {
        if (_isFocused == focused)
        {
            return;
        }

        _isFocused = focused;
        if (focused)
        {
            OnBillboardFocusOn?.Invoke();
        }
        else
        {
            OnBillboardFocusOff?.Invoke();
        }

        // 聚焦时切换为聚焦 Sprite，否则还原为初始 Sprite
        _key.sprite = focused ? _focusSprite : _initialSprite;
    }

    private void RestoreSprite()
    {
        if (_key != null)
        {
            _key.sprite = _initialSprite;
        }
    }

    public IArchitecture GetArchitecture() => TianArchitecture.Interface;
}
