using UnityEngine.UI.Windows.Components;

namespace UnityEngine.UI.Windows {

	[ComponentModuleDisplayName("Periodic rows coloring")]
	public class ListPeriodColoringComponentModule : ListComponentModule {

		public ImageComponent background;
		public int period;
		public Color firstColor;
		public Color secondColor;

		public override void OnShowBegin() {

			base.OnShowBegin();

			if(period == 0) return;

			var counter = 1;

			for (int i = 0; i < listComponent.items.Count; i++) {

				var imageObject = listComponent.items[i].transform.Find(this.background.name);

				if (counter <= this.period) {

					imageObject.GetComponent<ImageComponent>().SetColor(this.firstColor);

				}
				else {

					imageObject.GetComponent<ImageComponent>().SetColor(this.secondColor);

				}

				counter++;

				if (counter > this.period * 2) {

					counter = 1;

				}

			}

		}

	}

}
