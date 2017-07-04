using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace InfinitePainter
{
    public class MenuItem
    {
        public Symbol Icon
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }
    }

    public sealed partial class MainPage
    {
        private void InitialMenu()
        {
            var items = new List<MenuItem>();
            var option_items = new List<MenuItem>();
            items.Add(new MenuItem()
            {
                Icon = Symbol.BrowsePhotos,
                Name = "打开背景"
            });

            items.Add(new MenuItem()
            {
                Icon = Symbol.Document,
                Name = "加载涂鸦"
            });

            items.Add(new MenuItem()
            {
                Icon = Symbol.Save,
                Name = "保存涂鸦"
            });
            items.Add(new MenuItem()
            {
                Icon = Symbol.Send,
                Name = "导出图片"
            });

            option_items.Add(new MenuItem()
            {
                Icon = Symbol.Setting,
                Name = "全局设置"
            });
            option_items.Add(new MenuItem()
            {
                Icon = Symbol.Message,
                Name = "联系我们"
            });
            hamburgerMenuControl.ItemsSource = items;
            hamburgerMenuControl.OptionsItemsSource = option_items;
        }
    }
}
