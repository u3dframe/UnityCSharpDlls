using UnityEngine;
using System.Collections;

public class UGUIAutoRotat : MonoBehaviour
{
    public bool isOpen = false;      //是否开始旋转
    public int speed = 2;   //旋转的速度
    public SelfAxis axis = SelfAxis.Z;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isOpen)
        {

            RotateAxisOfSelf(SelfAxis.Z, speed);
        }

    }

    /// <summary>
    /// 让物体绕自身的轴旋转
    /// </summary>
    /// <param name="AxisX">自身的轴</param>
    private void RotateAxisOfSelf(SelfAxis selfAxis, int speed = 2)
    {
        switch (selfAxis)
        {
            case SelfAxis.X:
                this.transform.Rotate(new Vector3(1 * Time.deltaTime * speed, 0, 0));
                break;
            case SelfAxis.Y:
                this.transform.Rotate(new Vector3(0, 1 * Time.deltaTime * speed, 0));
                break;
            case SelfAxis.Z:
                this.transform.Rotate(new Vector3(0, 0, 1 * Time.deltaTime * speed));
                break;
            default:
                this.transform.Rotate(new Vector3(1 * Time.deltaTime * speed, 0, 0));
                break;

        }


    }


    //枚举轴
    public enum SelfAxis
    {
        X,
        Y,
        Z,

    }

}