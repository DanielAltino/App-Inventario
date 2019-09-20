using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Contexts;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Text;
using AppInventory.Controle;
using AppInventory.Model;
using AppInventory.Resources.DataBaseHelper;
using Environment = System.Environment;
using File = Java.IO.File;


namespace AppInventory
{
    [Activity(Label = "Inventário", MainLauncher = true, Icon = "@drawable/boxpS", ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    { 
        private bool _alterar;
        TextView _txtQtd;
        private EditText _txtPartNumber;
        private TextView _txtPartNumberHide;
        private TextView _lblUltimo;
        private DataBase _bd;
        private ListView _listView;
        private CheckBox _chkBusca;
        private Button _btnNovo;
        private Button _btnInserir;
        private ListViewAdapter _itemAdapter;
        private BroadcastReceiver _bReceiver;

        protected override void OnCreate(Bundle bundle)
        {

            try
            {

                base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            _alterar = false;

            CriaBancoDados();

            _txtPartNumber = FindViewById<EditText>(Resource.Id.txtPartNumber);
            _txtPartNumberHide = FindViewById<TextView>(Resource.Id.txtPartNumberHide);
            _txtQtd = FindViewById<TextView>(Resource.Id.txtQtd);
            _lblUltimo = FindViewById<TextView>(Resource.Id.lblUltLido);
            _txtPartNumber.InputType = 0;
            _txtPartNumberHide.InputType = 0;

            _chkBusca = FindViewById<CheckBox>(Resource.Id.chkBusca);
            _chkBusca.CheckedChange += ChkBuscaOnCheckedChange;

            _listView = FindViewById<ListView>(Resource.Id.lvDados);
            _listView.FastScrollEnabled = true;
            _listView.ItemClick += ListView_ItemClick;

            _btnNovo = FindViewById<Button>(Resource.Id.btnNovo);
            _btnInserir = FindViewById<Button>(Resource.Id.btnInserir);


            _btnNovo.Click += BtnNovoOnClick;
            _btnInserir.Click += BtnInserirOnClick;



            var btnLimpar = FindViewById<ImageButton>(Resource.Id.btnClean);
            btnLimpar.Click += btnLimpar_click;


            var btnExport = FindViewById<ImageButton>(Resource.Id.btnExport);
            btnExport.Click += BtnExport_Click;




            _txtPartNumber.KeyPress += (object sender, View.KeyEventArgs e) => {
                e.Handled = false;
                if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
                {
                    //add your logic here  
                    ReceiverOnLeitura(_txtPartNumber.Text);
                    e.Handled = true;
                }
            };


            CarregarDados();


        }
            catch (Exception ex)
            {
            };
}

        private void BtnExport_Click(object sender, EventArgs e)
        {
            Export();
        }

        private void ReceiverOnLeitura(object source)
        {
            var auxPn = source.ToString();

            if (auxPn != "")
            {
                if (_chkBusca.Checked == true)
                    CarregaItem(auxPn);
                else
                {
                    _txtQtd.Text = "1";
                    _lblUltimo.Text = _txtPartNumber.Text = source.ToString();

                    if (_txtQtd.Text == "")
                        _txtQtd.Text = "1";
                    SalvarPartNumber(auxPn);
                    LimpaTexto();
                }
            }
        }

        private void btnLimpar_click(object sender, EventArgs e)
        {
            LimparBase();
        }

        private void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            ClickItem(_itemAdapter[e.Position]);

        }

        private void ChkBuscaOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs checkedChangeEventArgs)
        {
            LimpaTexto();
            _btnInserir.Text = (sender as CheckBox).Checked == true ? "Salvar" : "Inserir";
            _alterar = (sender as CheckBox).Checked;

        }

        private void BtnInserirOnClick(object sender, EventArgs eventArgs)
        {
            if (_txtQtd.Text == "" && _txtPartNumber.Text != "")
                _txtQtd.Text = "1";

            SalvarPartNumber(_txtPartNumber.Text);

            LimpaTexto();
        }

        private void SalvarPartNumber(string pn)
        {
            if (pn == null)
                return;

            var auxItem = Procurar(pn);

            if (auxItem == null)
            {
                var item = new Item() { PartNumber = pn, Qtd = Convert.ToInt16(_txtQtd.Text) };
                Insert(item);
            }
            else
            {
                if (_alterar)
                    auxItem.Qtd = Convert.ToInt32(_txtQtd.Text);
                else
                    auxItem.Qtd += Convert.ToInt32(_txtQtd.Text);

                _alterar = false;
                Update(auxItem);
            }
            CarregarDados();
        }

        private void CarregaItem(string pn)
        {
            LimpaTexto();
            var auxItem = Procurar(pn);

            if (auxItem != null)
            {
                //_txtQtd.Focusable = true;
                _txtQtd.Selected = true;
                _txtPartNumber.Text = auxItem.PartNumber;
                _txtQtd.Text = auxItem.Qtd.ToString();
                _alterar = true;
            }
            else
            {
                Toast.MakeText(this, "Item não encontrado!", ToastLength.Long).Show();
            }
        }

        private void BtnNovoOnClick(object sender, EventArgs eventArgs)
        {
            LimpaTexto();
        }

        private void LimpaTexto()
        {
            _txtPartNumber.Text = "";
            _txtQtd.Text = "";
            _txtPartNumberHide.Selected = true;
            _txtPartNumberHide.RequestFocus();
            _txtPartNumberHide.InputType = 0;
        }

        #region DataBase 

        private void CriaBancoDados()
        {
            _bd = new DataBase();
            _bd.CriarBancoDeDados();
        }

        private void CarregarDados()
        {
            _itemAdapter = new ListViewAdapter(this, _bd.GetAll());
            _listView.Adapter = _itemAdapter;
        }

        private bool Update(Item item)
        {
            try
            {
                return _bd.AtualizarItem(item);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool Insert(Item item)
        {
            try
            {
                return _bd.InserirItem(item);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private Item Procurar(string partNumber)
        {
            try
            {
                return _bd.GetItem(partNumber);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private bool Excluir(Item partNumber)
        {
            try
            {
                return _bd.DeletarItem(partNumber);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool Excluir()
        {
            try
            {
                return _bd.Deletar();
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion

        private void ClickItem(Item item)
        {
            //define o alerta para executar a tarefa
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            AlertDialog alerta = builder.Create();
            //Define o Titulo
            alerta.SetTitle("Inventário - Você Deseja?");
            alerta.SetIcon(Android.Resource.Drawable.IcDialogAlert);
            alerta.SetMessage("Escolha a opção a baixo:");

            alerta.SetButton("Editar", (s, ev) =>
            {
                CarregaItem(item.PartNumber);

                Toast.MakeText(this, "Registro carregado... !", ToastLength.Short).Show();
            });
            alerta.SetButton2("Apagar", (s, ev) =>
            {
                Excluir(item);
                CarregarDados();
                Toast.MakeText(this, "Registro apagado... !", ToastLength.Short).Show();
            });
            alerta.SetButton3("Cancelar", (s, ev) =>
            {
                Toast.MakeText(this, "Vamos continuar... !", ToastLength.Short).Show();
            });
            alerta.Show();
        }

        private void LimparBase()
        {
            //define o alerta para executar a tarefa
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            AlertDialog alerta = builder.Create();
            //Define o Titulo
            alerta.SetTitle("Inventário - Limpar Registos");
            alerta.SetIcon(Android.Resource.Drawable.StatSysWarning);
            alerta.SetMessage("Todos os dados coletados serão ELIMINADOS, deseja Continuar?");


            alerta.SetButton2("Apagar", (s, ev) =>
            {
                Excluir();
                CarregarDados();
                Toast.MakeText(this, "Registros apagado... !", ToastLength.Short).Show();
            });
            alerta.SetButton3("Cancelar", (s, ev) =>
            {
                Toast.MakeText(this, "Vamos continuar... !", ToastLength.Short).Show();
            });
            alerta.Show();
        }


        private void Export()
        {
            var path = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + @"/Download";

            var file = System.IO.Path.Combine(path, "Inventario.txt");

            try
            {
                using (var streamWrite = new StreamWriter(file, false))
                {
                    foreach (var item in _bd.GetAll())
                    {
                        streamWrite.WriteLine(item.PartNumber + ";" + item.Qtd);
                    }
                }

                //define o alerta para executar a tarefa
                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                AlertDialog alerta = builder.Create();
                //Define o Titulo
                alerta.SetTitle("Registos Exportados!");
                alerta.SetIcon(Android.Resource.Drawable.StatSysWarning);
                alerta.SetMessage("Exportado para o Diretório Download. Deseja apagar os registros do sistema?");


                alerta.SetButton2("Apagar", (s, ev) =>
                {
                    Excluir();
                    CarregarDados();
                    Toast.MakeText(this, "Registros apagado... !", ToastLength.Short).Show();
                });
                alerta.SetButton3("Cancelar", (s, ev) =>
                {
                    Toast.MakeText(this, "Vamos continuar... !", ToastLength.Short).Show();
                });
                alerta.Show();
            }
            catch (Exception e)
            {

            }
        }


    

    }


}

