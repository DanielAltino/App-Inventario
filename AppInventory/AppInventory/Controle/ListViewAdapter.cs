using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AppInventory.Model;
using Object = Java.Lang.Object;

namespace AppInventory.Controle
{
    public class ListViewAdapter : BaseAdapter<Item>
    {
        private readonly Activity _context;

        private readonly List<Item> _itens;


        public ListViewAdapter(Activity context, List<Item> itens)
        {
            this._context = context;
            this._itens = itens;
        }

        public override long GetItemId(int position)
        {
            return _itens.Count;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView ?? _context.LayoutInflater.Inflate(Resource.Layout.ListViewLayout, parent, false);
            var lblPn = view.FindViewById<TextView>(Resource.Id.txtVItem);
            var lblQtd = view.FindViewById<TextView>(Resource.Id.txtVQtd);

            lblPn.Text = _itens[position].PartNumber;

            lblQtd.Text = _itens[position].Qtd.ToString();

            return view;
        }

        public override int Count { get { return _itens.Count; } }

        public override Item this[int position]
        {
            get { return _itens[position]; }
        }
    }
}