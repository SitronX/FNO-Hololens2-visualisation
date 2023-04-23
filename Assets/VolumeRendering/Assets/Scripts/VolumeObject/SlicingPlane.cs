using TMPro;
using UnityEngine;

namespace UnityVolumeRendering
{
    [ExecuteInEditMode]
    public class SlicingPlane : MonoBehaviour
    {
        public VolumeRenderedObject targetObject;
        private MeshRenderer meshRenderer;
        [SerializeField] TMP_Text _maxHU;
        [SerializeField] TMP_Text _minHU;
        [SerializeField] TMP_Text _maxHU2;
        [SerializeField] TMP_Text _minHU2;

        private void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        public void InitHounsfield(float min,float max)
        {
            _minHU.text = $"{min} <sprite=0>";
            _maxHU.text = $"{max} <sprite=0>";

            _minHU2.text = $"{min} <sprite=0>";
            _maxHU2.text = $"{max} <sprite=0>";
        }

        private void Update()
        {
            meshRenderer.sharedMaterial.SetMatrix("_parentInverseMat", transform.parent.worldToLocalMatrix);
            //meshRenderer.sharedMaterial.SetMatrix("_planeMat", Matrix4x4.TRS(transform.position, transform.rotation, transform.parent.lossyScale)); // TODO: allow changing scale
            meshRenderer.sharedMaterial.SetMatrix("_planeMat", transform.localToWorldMatrix);
        }
    }
}
