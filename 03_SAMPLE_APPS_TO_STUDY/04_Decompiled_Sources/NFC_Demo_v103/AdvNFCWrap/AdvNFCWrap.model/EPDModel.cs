using System.Collections.Generic;

namespace AdvNFCWrap.model
{
	internal sealed class EPDModel
	{
		private static volatile EPDModel instance = null;

		private static object syncObj = new object();

		private Dictionary<string, Dictionary<string, object>> config = new Dictionary<string, Dictionary<string, object>>();

		public static string COLOR_BW = "BW";

		public static string COLOR_RBW = "RBW";

		public static string COLOR_GRAY = "GRAY";

		public static string COLOR_FOUR = "FOUR";

		public static string COLOR_FULL = "FULL";

		public static string COLOR_SEVEN = "SEVEN";

		public static EPDModel Instance
		{
			get
			{
				if (instance == null)
				{
					lock (syncObj)
					{
						if (instance == null)
						{
							instance = new EPDModel();
						}
					}
				}
				return instance;
			}
		}

		private EPDModel()
		{
			Dictionary<string, object> value = new Dictionary<string, object>
			{
				{
					"width",
					296
				},
				{
					"height",
					128
				},
				{
					"color",
					COLOR_BW
				},
				{
					"max_page",
					1
				},
				{
					"image_reverse",
					true
				}
			};
			config.Add("EPD-210", value);
			value = new Dictionary<string, object>
			{
				{
					"width",
					416
				},
				{
					"height",
					240
				},
				{
					"color",
					COLOR_BW
				},
				{
					"max_page",
					1
				},
				{
					"image_reverse",
					true
				}
			};
			config.Add("EPD-302", value);
			value = new Dictionary<string, object>
			{
				{
					"width",
					416
				},
				{
					"height",
					240
				},
				{
					"color",
					COLOR_RBW
				},
				{
					"max_page",
					1
				},
				{
					"image_reverse",
					true
				}
			};
			config.Add("EPD-303", value);
			value = new Dictionary<string, object>
			{
				{
					"width",
					416
				},
				{
					"height",
					240
				},
				{
					"color",
					COLOR_FOUR
				},
				{
					"max_page",
					1
				},
				{
					"image_reverse",
					true
				}
			};
			config.Add("EPD-304", value);
		}

		public Dictionary<string, object> getProperty(string model)
		{
			Dictionary<string, object> value = null;
			config.TryGetValue(model, out value);
			return value;
		}
	}
}
