namespace UnityEngine.UI.Windows {

    public class TextComponentTextLinkModule : TextComponentModule, UnityEngine.EventSystems.IPointerClickHandler {

        public string id;
        public string link;
        
        public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData) {
            #if TEXTMESHPRO_SUPPORT
            if (this.textComponent.graphics is TMPro.TMP_Text text) {
                var index = TMPro.TMP_TextUtilities.FindIntersectingCharacter(text, WindowSystem.GetPointerPosition(), this.GetWindow().workCamera, true);
                if (index < 0) return;
                var link = text.textInfo.linkInfo[index];
                if (link.GetLinkID() == this.id) {
                    Application.OpenURL(this.link);
                }
            }
            #endif
        }
        
    }
    
}
