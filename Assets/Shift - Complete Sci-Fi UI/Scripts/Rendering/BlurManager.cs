using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Michsky.UI.Shift
{
    public class BlurManager : MonoBehaviour
    {
        [Header("Resources")]
        public Material blurMaterial;

        [Header("Settings")]
        // [Range(0.0f, 10)] public float blurValue = 5.0f;
        // [Range(0.1f, 50)] public float animationSpeed = 25;
        float blurValue = 1.5f;
        float animationSpeed = 15;
        public string customProperty = "_Size";

        float currentBlurValue;


        void Awake()
        {
            if (blurMaterial != null)
            {
                blurMaterial = new Material(blurMaterial);
                GetComponent<Image>().material = blurMaterial;
            }
        }


        void Start()
        {
            if (customProperty == null)
                customProperty = "_Size";

            blurMaterial.SetFloat(customProperty, 0);
        }

        IEnumerator BlurIn()
        {
            currentBlurValue = blurMaterial.GetFloat(customProperty);

            if (currentBlurValue >= 0)
                currentBlurValue = 0;

            while (currentBlurValue < blurValue)
            {
                currentBlurValue += Time.deltaTime * animationSpeed;

                if (currentBlurValue >= blurValue)
                    currentBlurValue = blurValue;

                blurMaterial.SetFloat(customProperty, currentBlurValue);
                yield return null;
            }
        }

        IEnumerator BlurOut()
        {
            currentBlurValue = blurMaterial.GetFloat(customProperty);

            while (currentBlurValue > 0)
            {
                currentBlurValue -= Time.deltaTime * animationSpeed;

                if (currentBlurValue <= 0)
                    currentBlurValue = 0;

                blurMaterial.SetFloat(customProperty, currentBlurValue);
                yield return null;
            }
        }

        public void BlurInAnim()
        {
            if (gameObject.activeInHierarchy == false)
                return;

            StopCoroutine(nameof(BlurOut));
            StartCoroutine(nameof(BlurIn));
        }

        public void BlurOutAnim()
        {
            if (gameObject.activeInHierarchy == false)
                return;

            StopCoroutine(nameof(BlurIn));
            StartCoroutine(nameof(BlurOut));
        }

        public void SetBlurValue(float cbv)
        {
            blurValue = cbv;
        }
    }
}