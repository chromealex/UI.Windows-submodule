namespace UnityEngine.UI.Windows {

    public class HoverComponentModule : ButtonComponentModule, UnityEngine.EventSystems.IPointerEnterHandler, UnityEngine.EventSystems.IPointerExitHandler {

        [UnityEngine.UI.Windows.Utilities.RequiredReferenceAttribute]
        public WindowComponent content;

        public override void ValidateEditor() {
            
            base.ValidateEditor();

            if (this.content != null) {

                this.content.hiddenByDefault = true;
                this.content.AddEditorParametersRegistry(new WindowObject.EditorParametersRegistry(this) {
                    holdHiddenByDefault = true,
                });

            }

        }

        public override void OnInit() {
            
            base.OnInit();

            WindowSystem.onPointerUp += this.OnPointerUp;

        }

        public override void OnDeInit() {
            
            WindowSystem.onPointerUp -= this.OnPointerUp;
            
            base.OnDeInit();
            
        }

        private void OnPointerUp() {
            
            if (this.content != null) this.content.Hide();

        }

        public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData) {

            if (this.content != null) this.content.Show();

        }
        
        public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData) {

            if (this.content != null) this.content.Hide();

        }

    }

}
