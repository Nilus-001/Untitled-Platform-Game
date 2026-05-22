using UnityEngine;

public class ProgressBarToCenter : MonoBehaviour{
    [SerializeField] private RectTransform bar  ;
    [Range(0f, 1f)] public float value = 1f;


    float baseWidth = 0f;
    protected void Start() {
        baseWidth = bar.sizeDelta.x;
    }

    protected void Update(){
        SetProgress();
    }

    private void  SetProgress() {
        float v = Mathf.Clamp01(value);
        bar.sizeDelta = new Vector2(v*baseWidth,bar.sizeDelta.y);
    }

}
