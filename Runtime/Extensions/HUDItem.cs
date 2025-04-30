using UnityEngine;

public class HUDItem : MonoBehaviour {

    public Camera uiCamera;
    public Camera gameCamera;
    public Transform alignTo;

    public bool pointAligned = false;
    public Vector3 alignToPoint;

    public Vector3 offset;

    private bool isVisible;

    public void InitHUD(Transform alignTo, Camera uiCamera, Camera gameCamera, Vector3 offset = default) {

        this.uiCamera = uiCamera;
        this.gameCamera = gameCamera;

        if (offset != default) {
            this.offset = offset;
        }

        this.alignTo = alignTo;
        this.pointAligned = false;

        this.transform.localRotation = Quaternion.identity;
        this.transform.localScale = Vector3.one;

        this.Reposition();

    }

    public void InitHUD(Vector3 alignToPoint, Camera uiCamera, Camera gameCamera, Vector3 offset = default) {

        this.uiCamera = uiCamera;
        this.gameCamera = gameCamera;

        if (offset != default) {
            this.offset = offset;
        }

        this.alignToPoint = alignToPoint;
        this.pointAligned = true;

        this.transform.localRotation = Quaternion.identity;
        this.transform.localScale = Vector3.one;

        this.Reposition();

    }

    public void SetAlignToPoint(Vector3 point) {

        this.alignToPoint = point;
        this.pointAligned = true;

        this.Reposition();

    }

    public void Reset() {

        this.alignTo = null;
        this.uiCamera = null;
        this.gameCamera = null;
        this.pointAligned = false;

    }

    public bool IsVisible() {

        return this.isVisible;

    }

    public void SetVisible(bool state) {

        this.isVisible = state;

    }

    public void LateUpdate() {

        if (this.pointAligned == true) {
            return;
        }

        if ((this.alignTo != null || this.pointAligned == true) && this.uiCamera != null && this.gameCamera != null) {

            this.Reposition();

        }

    }

    public static void Reposition(
        Transform target,
        Vector3 sourcePos,
        Camera gameCamera,
        Camera uiCamera,
        Vector2 offset = default) {

        var pos = gameCamera.WorldToViewportPoint(sourcePos);
        var rPos = uiCamera.ViewportToWorldPoint(pos);
        rPos.z = target.position.z;
        var v1 = new Vector2(target.position.x, target.position.y);
        var v2 = new Vector2(rPos.x, rPos.y);
        if (v1 != v2) {

            target.position = rPos;
            target.localPosition = new Vector3(target.localPosition.x + offset.x, target.localPosition.y + offset.y, 0f);

        }

    }

    public static Vector3 Reposition(Vector3 sourcePos, Camera gameCamera, Camera uiCamera) {

        var target = Vector3.zero;
        if (uiCamera.orthographicSize > 0f) {
            var pos = gameCamera.WorldToViewportPoint(sourcePos);
            pos.z = 0f;
            var rPos = uiCamera.ViewportToWorldPoint(pos);
            rPos.z = target.z;
            target = rPos;
        }

        return target;

    }

    public bool debug;

    public void Reposition() {

        if (this.alignTo != null || this.pointAligned == true) {

            if (this.uiCamera.orthographicSize <= 0f) return;

            var position = this.pointAligned == true ? this.alignToPoint + this.offset : this.alignTo.transform.position + this.offset;
            var pos = this.gameCamera.WorldToViewportPoint(position);

            var isVisible = pos.z > 0f && pos.x > 0f && pos.x < 1f && pos.y > 0f && pos.y < 1f;

            if (this.isVisible != isVisible) {

                this.SetVisible(isVisible);

            }

            var rPos = this.uiCamera.ViewportToWorldPoint(pos);
            var v1 = new Vector2(this.transform.position.x, this.transform.position.y);
            var v2 = new Vector2(rPos.x, rPos.y);
            if (v1 != v2) {

                if (this.debug == true) {
                    var w = this.GetComponentInParent<UnityEngine.UI.Windows.WindowBase>();
                    var tr = w.GetCanvas().transform;
                    Debug.Log("tr1: " + tr.localPosition);
                    Debug.Log("tr2: " + tr.localRotation);
                    Debug.Log("tr3: " + tr.localScale);
                }

                this.transform.position = rPos;
                var localPos = this.transform.localPosition;
                localPos.z = 0f;
                this.transform.localPosition = localPos;

            }

        }

    }

}