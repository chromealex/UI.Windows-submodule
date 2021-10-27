using System.Collections.Generic;

namespace UnityEngine.UI.Windows {
    
    using Utilities;
    
    [ComponentModuleDisplayName("Arrangement")]
    public class ListArrangementComponentModule : ListComponentDraggableModule {

        public enum Type {

            Circle,
            Carousel,

        }

        public ScrollRect scrollRect;
        [RequiredReference]
        public RectTransform view;
        [RequiredReference]
        public RectTransform root;
        public Type type;
        public float moveToTargetSpeed = 10f;
        public float movementFactor = 40f;
        public int targetIndex;

        private Vector2 targetPosition;
        private bool isDragging;
        
        // Circle
        [System.Serializable]
        public struct CircleParameters {

	        public enum ArrangementDirection {

		        Clockwise,
		        CounterClockwise,

	        }

	        public enum ArrangementSide {

		        OneSide,
		        BothSided,
		        BothSidedSorted,

	        }

	        public ArrangementDirection direction;
	        public ArrangementSide side;
            public float startAngle;
            public float maxAngle;
            public float maxAnglePerElement;
            public float xRadiusOffset;
            public float yRadiusOffset;
            public float iterationXSizeFactor;
            public float iterationYSizeFactor;
            public bool alignRotation;
            public float rotationAngle;

        }

        public CircleParameters circleParameters = new CircleParameters() {
            startAngle = 0f,
            maxAngle = 360f,
            maxAnglePerElement = 360f,
            xRadiusOffset = 0f,
            yRadiusOffset = 0f,
            iterationXSizeFactor = 0f,
            iterationYSizeFactor = 0f,
            alignRotation = false,
        };

        // Carousel
        [System.Serializable]
        public struct CarouselParameters {

	        public float directionAngle;
	        public Vector2 spacing;

        }

        public CarouselParameters carouselParameters = new CarouselParameters() {
	        directionAngle = 0f,
        };
        
        protected DrivenRectTransformTracker tracker;
        
        public override void ValidateEditor() {
            
            base.ValidateEditor();
            
            if (this.scrollRect == null) this.scrollRect = this.windowComponent.GetComponent<ScrollRect>();
            if (this.view == null && this.scrollRect != null) this.view = this.scrollRect.transform as RectTransform;
            if (this.root == null && this.scrollRect != null) this.root = this.scrollRect.content;
            
            this.tracker.Clear();
            this.CollectChildren();
            this.Arrange();
            
        }

        public override void OnBeginDrag(UnityEngine.EventSystems.PointerEventData data) {
	        
	        this.isDragging = true;

        }

        public override void OnDrag(UnityEngine.EventSystems.PointerEventData data) {
	        
        }

        public override void OnEndDrag(UnityEngine.EventSystems.PointerEventData data) {
	        
	        this.isDragging = false;

        }

        public override void OnInit() {
	        
	        base.OnInit();

	        if (this.listComponent.customRoot == null) this.listComponent.customRoot = this.root;
	        if (this.scrollRect != null) this.scrollRect.onValueChanged.AddListener(this.OnScrollValueChanged);
	        
        }

        public override void OnDeInit() {
	        
	        base.OnDeInit();
	        
	        if (this.scrollRect != null) this.scrollRect.onValueChanged.RemoveListener(this.OnScrollValueChanged);

        }

        public override void OnShowBegin() {
	        
	        base.OnShowBegin();
	        
	        this.Arrange();
	        
        }

        public override void OnShowEnd() {
	        
	        base.OnShowEnd();

	        this.targetIndex = 0;

        }

        private void OnScrollValueChanged(Vector2 position) {

	        if (this.isDragging == false) return;

	        var velocity = this.scrollRect.velocity.normalized;
	        
	        var size = this.root.rect.size;
	        var viewSize = this.view.rect.size;
	        var pos2d = new Vector2(position.x * size.x - size.x * 0.5f, position.y * size.y - size.y * 0.5f);
	        var offset = new Vector2(-viewSize.x * 0.5f, -viewSize.y * 0.5f) + velocity * this.movementFactor;
	        
	        var idx = -1;
	        RectTransform nearest = null;
	        var d = float.MaxValue;
	        var currentDistance = 0f;
	        for (int i = 0; i < this.listComponent.items.Count; ++i) {

		        var dist = (this.listComponent.items[i].rectTransform.anchoredPosition + offset - pos2d).sqrMagnitude;
		        if (i == this.targetIndex) {

			        currentDistance = dist;

		        }
		        if (dist < d) {

			        d = dist;
			        nearest = this.listComponent.items[i].rectTransform;
			        idx = i;

		        }

	        }

	        if (idx >= 0) {

		        if (currentDistance > d) {

			        var p = nearest.anchoredPosition;
			        this.targetPosition = new Vector2(-p.x, -p.y);
			        this.targetIndex = idx;

		        }

	        }

        }

        private void CollectChildren() {
	        
        }
        
        public override void OnComponentsChanged() {
	        
	        base.OnComponentsChanged();

	        this.CollectChildren();
	        this.Arrange();
	        
        }

        public void Arrange() {

            //var rect = this.root.rect;
            var childs = this.listComponent.items;

            var center = Vector2.zero;
            var minMaxRect = new Rect();
            var count = childs.Count;
            for (int c = 0; c < 2; ++c) {

	            var k = 0;
	            this.lastIndex = 0;

	            for (int i = 0; i < count; ++i) {

		            var tr = childs[i];
		            var p = this.GetPosition(childs, k++, count, this.view.rect.size, out var rotation, out var rotationChanged);

		            if (c == 1) {

			            var middle = (count > 1 ? center / count : Vector2.zero);
			            tr.rectTransform.anchoredPosition = new Vector2(p.x - middle.x, p.y - middle.y);
			            this.tracker.Add(this, tr.rectTransform, DrivenTransformProperties.AnchoredPosition);
			            
			            if (rotationChanged == true) {
				            
				            tr.rectTransform.localRotation = rotation;
				            this.tracker.Add(this, tr.rectTransform, DrivenTransformProperties.Rotation);
				            
			            }

		            } else if (c == 0) {

			            var itemRect = tr.rectTransform.rect;
			            
			            center += new Vector2(p.x, p.y);
			            
			            minMaxRect.xMin = Mathf.Min(minMaxRect.xMin, p.x - itemRect.width * 0.5f);
			            minMaxRect.xMax = Mathf.Max(minMaxRect.xMax, p.x + itemRect.width * 0.5f);
			            minMaxRect.yMin = Mathf.Min(minMaxRect.yMin, p.y - itemRect.height * 0.5f);
			            minMaxRect.yMax = Mathf.Max(minMaxRect.yMax, p.y + itemRect.height * 0.5f);

		            }

	            }
	            
            }

            this.tracker.Add(this, this.root, DrivenTransformProperties.SizeDelta);

            var rectView = this.view.rect;
            this.root.sizeDelta = new Vector2(minMaxRect.width + rectView.width, minMaxRect.height + rectView.height);
            
        }

        private void ClampToTarget() {

	        this.targetIndex = Mathf.Clamp(this.targetIndex, 0, this.listComponent.items.Count);
	        {

		        var dt = Time.deltaTime;
		        this.root.anchoredPosition = Vector2.Lerp(this.root.anchoredPosition, this.targetPosition, dt * this.moveToTargetSpeed);

	        }
	        
        }
        
        public void LateUpdate() {

	        if (this.isDragging == false) {

		        this.ClampToTarget();
		        //this.UpdateSelected();

	        }

        }

        public Vector2 GetPosition(List<WindowComponent> childs, int index, int count, Vector2 size, out Quaternion rotation, out bool rotationChanged) {

            var pos = Vector2.zero;
            rotation = Quaternion.identity;
            rotationChanged = false;
            switch (this.type) {

                case Type.Circle: {

	                var radius = new Vector2(size.x * 0.5f, size.y * 0.5f);
	                var reverse = (this.circleParameters.direction == CircleParameters.ArrangementDirection.CounterClockwise);
	                pos = this.GetPositionByCircle(
		                (reverse == true ? count - index : index),
		                count,
		                radius.x + this.circleParameters.xRadiusOffset,
		                radius.y + this.circleParameters.yRadiusOffset,
		                this.circleParameters.side,
		                this.circleParameters.startAngle,
		                this.circleParameters.maxAnglePerElement,
		                this.circleParameters.maxAngle);

	                if (this.circleParameters.alignRotation == true) {
		                
		                var angle = Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg;
		                rotation = Quaternion.Euler(0f, 0f, angle - 90f + this.circleParameters.rotationAngle);
		                rotationChanged = true;

	                }

                }
	                break;

                case Type.Carousel: {

	                if (childs.Count > 0) {

		                var firstSize = childs[0].rectTransform.rect.size;
		                
		                var w = 0f;
		                var h = 0f;
		                for (int i = 0; i < childs.Count; ++i) {

			                if (i >= index) break;

			                var s = childs[i].rectTransform.rect.size;
			                w += s.x + this.carouselParameters.spacing.x;
			                h += s.y + this.carouselParameters.spacing.y;

		                }

		                w -= firstSize.x * 0.5f;
		                w += childs[index].rectTransform.rect.size.x * 0.5f;

		                h -= firstSize.y * 0.5f;
		                h += childs[index].rectTransform.rect.size.y * 0.5f;

		                var x = w;
		                var y = h;
		                pos = new Vector2(x, y);

		                var radiusX = Mathf.Abs(pos.x);
		                var radiusY = Mathf.Abs(pos.y);
		                var angle = this.carouselParameters.directionAngle * Mathf.Deg2Rad;
		                var position = Vector2.zero;
		                position.x = Mathf.Sin(angle) * radiusX * Mathf.Sign(pos.x);
		                position.y = Mathf.Cos(angle) * radiusY * Mathf.Sign(pos.y);
		                pos = position;

	                }
	                
                }
	                break;
                
            }
            return pos;

        }
        
        private int lastIndex = 0;
		private Vector2 GetPositionByCircle(int index, int count, float radiusX, float radiusY, CircleParameters.ArrangementSide side, float startAngle, float maxAnglePerElement, float maxAngle) {
			
			if (count <= 0) {

				return Vector2.zero;

			}

			var elementAngle = maxAngle / count;
			var offset = false;
			
			var non360 = maxAngle < 360f;
			
			offset = (elementAngle >= maxAnglePerElement);
			
			if (side == CircleParameters.ArrangementSide.BothSided || side == CircleParameters.ArrangementSide.BothSidedSorted) {

				if (side == CircleParameters.ArrangementSide.BothSidedSorted) {

					var elementAngleWithOffset = (offset == true ? maxAnglePerElement : elementAngle);
					if (count % 2 != 0) startAngle -= elementAngleWithOffset * 0.5f;
					startAngle -= elementAngleWithOffset * count * 0.5f - elementAngleWithOffset * 0.5f;

				} else {

					if (index % 2 == 0) {
						
						index = this.lastIndex - index;
						
					} else {
						
						index = this.lastIndex + index;
						
					}

					var elementAngleWithOffset = (offset == true ? maxAnglePerElement : elementAngle);
					if (count % 2 == 0) startAngle -= elementAngleWithOffset * 0.5f;

				}

			} else {

				if (non360 == true) {
					
					//--index;
					--count;
					//count -= 1 * Mathf.FloorToInt(maxAngle / 180f);

				}
				//--count;
				
			}
			
			if (offset == true) {
				
				maxAngle *= maxAnglePerElement / elementAngle;
				
			}

			if (count == 0) {

				count = 1;

			}

			maxAngle = maxAngle * Mathf.Deg2Rad;
			startAngle = startAngle * Mathf.Deg2Rad;
			var position = Vector2.zero;
			position.x = Mathf.Sin(maxAngle / count * index + startAngle) * (radiusX + (this.circleParameters.iterationXSizeFactor * 360f) * index * (maxAngle / 360f));
			position.y = Mathf.Cos(maxAngle / count * index + startAngle) * (radiusY + (this.circleParameters.iterationYSizeFactor * 360f) * index * (maxAngle / 360f));

			this.lastIndex = index;
			
			return new Vector2(position.x, position.y);
			
		}
		
    }

}
