
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleButton : MonoBehaviour
{
    [SerializeField]
    private Graphic offGraphic;

    void Start()
    {
        Toggle toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener((value) => 
        {
            OnValueChanged(value);
        });

        //初期状態を反映
        if (offGraphic != null) offGraphic.enabled = !toggle.isOn;
    }

    void OnValueChanged(bool value)
    {
        if (offGraphic != null)
        {
            offGraphic.enabled = !value;
        }
    }
}