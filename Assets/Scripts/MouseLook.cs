using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float sensitivity = 2f;
    public float clampAngle = 80f;

    private float _rotX;
    private Transform _playerBody;

    void Start()
    {
        _rotX = transform.localEulerAngles.x;
        _playerBody = transform.parent?.parent;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        _rotX -= mouseY;
        _rotX = Mathf.Clamp(_rotX, -clampAngle, clampAngle);

        transform.localEulerAngles = new Vector3(_rotX, 0f, 0f);

        if (_playerBody)
            _playerBody.Rotate(Vector3.up * mouseX);
    }
}
