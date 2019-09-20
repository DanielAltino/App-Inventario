using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace AppInventory.Controle
{
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { "kr.co.bluebird.android.bbapi.action.BARCODE_CALLBACK_DECODING_DATA" })]
    public class LaserReciver : BroadcastReceiver
    {

        public override void OnReceive(Context context, Intent intent)
        {

            var cod = intent.GetByteArrayExtra("EXTRA_BARCODE_DECODING_DATA");

            var codBarra = System.Text.Encoding.UTF8.GetString(cod);

            if (Leitura != null)
                Leitura(codBarra);

        }

        public delegate void DadoLido(object source);

        public event DadoLido Leitura;

    }
}