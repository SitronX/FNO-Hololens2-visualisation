using TMPro;
using UnityEngine;

namespace UnityVolumeRendering
{
    [ExecuteInEditMode]
    public class SlicingPlane : MonoBehaviour
    {
        public VolumeRenderedObject targetObject;
        [SerializeField] MeshRenderer _meshRenderer;
        [SerializeField] TMP_Text _maxHU;
        [SerializeField] TMP_Text _minHU;
        [SerializeField] TMP_Text _maxHU2;
        [SerializeField] TMP_Text _minHU2;
        [SerializeField] Transform _parentMatrix;

        
        public void UpdateHounsfieldWindow(float lower,float upper,float minHU,float maxHU)
        {
            int min = Utils.GetHUFromFloat(lower, minHU, maxHU);
            int max= Utils.GetHUFromFloat(upper, minHU, maxHU);

            _minHU.text = $"{min} <sprite=0>";
            _maxHU.text = $"{max} <sprite=0>";

            _minHU2.text = $"{min} <sprite=0>";
            _maxHU2.text = $"{max} <sprite=0>";

            _meshRenderer.sharedMaterial.SetFloat("minVal", lower);
            _meshRenderer.sharedMaterial.SetFloat("maxVal", upper);
        }

        private void Update()
        {
            _meshRenderer.sharedMaterial.SetMatrix("_parentInverseMat", _parentMatrix.worldToLocalMatrix);
            //meshRenderer.sharedMaterial.SetMatrix("_planeMat", Matrix4x4.TRS(transform.position, transform.rotation, transform.parent.lossyScale)); // TODO: allow changing scale
            _meshRenderer.sharedMaterial.SetMatrix("_planeMat", transform.localToWorldMatrix);
        }
    }
}
