using UnityEngine;
using UnityEngine.UI;

public class CameraAspectController : MonoBehaviour
{
    float targetAspect = 16f / 9f; // 원하는 화면 비율
    Color letterboxColor = Color.gray; // 레터박스 색상
    Sprite letterboxImage; // 레터박스 이미지 (선택사항)

    void Start()
    {
        // 배경 카메라 생성 (레터박스 배경용)
        GameObject bgCamObj = new GameObject("Letterbox Background Camera");
        Camera bgCam = bgCamObj.AddComponent<Camera>();
        bgCam.depth = -100; // 메인 카메라보다 먼저 렌더링
        bgCam.clearFlags = CameraClearFlags.SolidColor;
        bgCam.backgroundColor = letterboxColor;
        bgCam.cullingMask = 1 << LayerMask.NameToLayer("UI"); // UI 레이어만 렌더링

        // 배경 이미지가 있으면 추가
        if (letterboxImage != null)
        {
            CreateLetterboxImageUI(bgCamObj);
        }

        // 메인 카메라 설정
        Camera camera = GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.Depth; // 배경 카메라 위에 렌더링

        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1.0f)
        {
            // 레터박스 (위아래 검은 바)
            Rect rect = camera.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            camera.rect = rect;
        }
        else
        {
            // 필러박스 (좌우 검은 바)
            float scalewidth = 1.0f / scaleHeight;
            Rect rect = camera.rect;
            rect.width = scalewidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scalewidth) / 2.0f;
            rect.y = 0;
            camera.rect = rect;
        }
    }

    void CreateLetterboxImageUI(GameObject bgCamObj)
    {
        // Canvas 생성
        GameObject canvasObj = new GameObject("Letterbox Image Canvas");
        canvasObj.layer = LayerMask.NameToLayer("UI");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = bgCamObj.GetComponent<Camera>();
        canvas.planeDistance = 10;
        canvas.sortingOrder = -1;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // 이미지 오브젝트 생성
        GameObject imageObj = new GameObject("Background Image");
        imageObj.layer = LayerMask.NameToLayer("UI");
        imageObj.transform.SetParent(canvasObj.transform, false);

        RectTransform rectTransform = imageObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;

        Image image = imageObj.AddComponent<Image>();
        image.sprite = letterboxImage;
        image.color = Color.white;
    }
}