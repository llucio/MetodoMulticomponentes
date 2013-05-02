using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MetodoMulticomponentes
{
    public partial class Form1 : Form
    {//estas son todas las variables que se utilizan
        private int Xm;
        private int Ym;
        int CL = 0;
        int CP = 0;
        double fmdcp = 0;
        double fmrcp = 0;
        double Nmin = 0,Net=0;
        double[] FraccionM;
        double[] WFraccion;
        double Ptotal = 0;
        double Q = 0;
        double Re = 0;
        double WFracctotal = 0,FFracciontotal=0;
        double Reop = 0;
        double Nze = 0, Nza = 0;

        public Form1()
        {
            InitializeComponent();
        }
        //esta funcion no hace nada je
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        //esta funcion obtiene el calor removido con el condensador 
        private void calorRemovido()
        {
            double lambda = Convert.ToDouble(textBox9.Text);
            double V = (Reop+1)*FFracciontotal;
            double Qc = lambda * V;
            textBox6.Text = Qc.ToString("#,0.0000");
        }
        //se obtiene Nze y Nza
        private void kirkbride()
        {
            
            double relacion=Math.Pow((Convert.ToDouble(dataGridView1.Rows[CP].Cells[2].Value)/Convert.ToDouble(dataGridView1.Rows[CL].Cells[2].Value))*
                            Math.Pow((WFraccion[CL]/FraccionM[CP]),2)*(WFracctotal/FFracciontotal),0.206);//relacion =Nze/Nza
            Nza = Net / (relacion + 1.0);
            Nze = relacion * Nza;
            textBox4.Text = Nza.ToString("#,0.0000");
            textBox5.Text = Nze.ToString("#,0.0000");
        }
        //esta funcion eobtiene el valor mas optimo de teta
        private void underwood()
        {
            double teta = 0;
            double Remin = 0;
            var tabla=new Dictionary<double,double>();
            Q = Convert.ToDouble(textBox7.Text);
            Re = Convert.ToDouble(textBox8.Text);
            double hk = Convert.ToDouble(dataGridView1.Rows[CP].Cells[9].Value);
            double lk = Convert.ToDouble(dataGridView1.Rows[CL].Cells[9].Value);
            double sol = 1.0 - Q;
            do{
                hk += 0.001;//teta se incrementa en .001cada vez
                double fun = funderwood(hk);
                if(fun>sol)tabla.Add(fun,hk);
                
            }while(hk<lk);
           double min=double.MaxValue;
            double closest=0;
            foreach (double ft in tabla.Keys)
            {
                if((ft-sol)<min){
                    min = ft - sol;
                    closest = ft;
                }
            }
            tabla.TryGetValue(closest, out teta);// se encuentra el valor mas optimo de teta
            Remin = sunderwood(teta) - 1.0;//se aplica la segunda ecuacion de underwood para obtener Remin
            Reop = Convert.ToDouble(textBox8.Text) * Remin;//se obtiene Reop

            double gulx = (Reop - Remin) / (Reop + 1.0);//se obtiene el valor de x para evaluar en la grafica de guilliland
            double guly = 0.75*(1 - Math.Pow(gulx, 0.5668));//se evalua en la grafica (en este caso es una funcion aproximada) y e obtioene el valor de y
            Net = (guly + Nmin) / (1 - guly);//y asi se obtiene el numero de etapas totales
            textBox3.Text = Net.ToString("#,0.0000");//y se muestran en el cuadrito de texto
        }
        //esta funcion corresponde a la primera ecuacion de underwood
        private double funderwood(double t)
        {
            double u=0;
            for(int i=0;i<dataGridView1.Rows.Count-1;i++)
            {
                u += (Convert.ToDouble(dataGridView1.Rows[i].Cells[9].Value) * Convert.ToDouble(dataGridView1.Rows[i].Cells[2].Value)) / (Convert.ToDouble(dataGridView1.Rows[i].Cells[9].Value)-t);
            }
            return u;
        }
        //esta funcion corresponde a la segunda ecuacion de underwood
        private double sunderwood(double t)
        {
            double u = 0;
            for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
            {
                u += (Convert.ToDouble(dataGridView1.Rows[i].Cells[9].Value) * FraccionM[i]) / (Convert.ToDouble(dataGridView1.Rows[i].Cells[9].Value) - t);
            }
            return u;
        }
        //esta funcion obtiene los Wx y Dx(Fraccion molar)
        private void recalculo()
        {

            double[] w= new double[dataGridView1.Rows.Count-1];
            double[] d= new double[dataGridView1.Rows.Count-1];
            double[] f = new double[dataGridView1.Rows.Count-1];
            double[] W = new double[dataGridView1.Rows.Count-1];
            for (int i = 0; i < w.Length; i++)
            {
                w[i]=Convert.ToDouble(dataGridView1.Rows[i].Cells[1].Value)/(1+Math.Pow(Convert.ToDouble(dataGridView1.Rows[i].Cells[9].Value),Nmin)*(fmdcp/fmrcp));
                WFracctotal += w[i];
            }
            WFraccion = new double[W.Length];
  
            for (int i = 0; i < W.Length; i++)
            {
                W[i] = w[i] / WFracctotal;
                WFraccion[i] = W[i];
            }
            for (int i = 0; i < d.Length; i++)
            {
                d[i] = Convert.ToDouble(dataGridView1.Rows[i].Cells[1].Value) - w[i];
                FFracciontotal += d[i];
            }
            FraccionM = new double[f.Length];
            for (int i = 0; i < f.Length; i++)
            {
                f[i] = d[i] / FFracciontotal;
                FraccionM[i] = f[i];
            }
          
        }
        //Esta fucnion obtiene la presion de operacion despues de hacer el recalculo
        private void presionOp()
        {
            recalculo();
            double Pdomo = 0;
            double Pfondo = 0;
            for (int i = 0; i < dataGridView1.Rows.Count-1; i++)
            {
                Pdomo += Convert.ToDouble(dataGridView1.Rows[i].Cells[4].Value) * FraccionM[i];
                Pfondo += Convert.ToDouble(dataGridView1.Rows[i].Cells[7].Value) * WFraccion[i];
            }
            Ptotal = (Pdomo + Pfondo) / 2.0;
            textBox2.Text = Ptotal.ToString("#,0.0000");//estos numeros extraños solo le dicen al programa que oculte
            textBox10.Text = WFracctotal.ToString("#,0.0000");//los demas lugares decimales y solo muestre cuatro decimales
            textBox11.Text = FFracciontotal.ToString("#,0.0000");
            listBox1.DataSource = WFraccion;
            listBox2.DataSource = FraccionM;

        }
        //ecuacion de fenske
        private double fenske(double a, double b, double c, double d, double e)
        {
            double nmin = 0;
            nmin=Math.Log((a/b)*(c/d))/Math.Log(e);
           return nmin;

        }
        private void getclaves()//caclula el numero minimo de etapas
        {
            double a=0,b=0,c=0,d=0,e=0;
            int filas =dataGridView1.Rows.Count-1;
            for (int i = 0; i < filas; i++)
            {
                //itera las filas que tengan flujo molar residuo para enxcontrar el clave ligero y el clave pesado
                if (System.Convert.ToDouble(dataGridView1.Rows[i].Cells[6].Value) > 0)
                {
                    b = System.Convert.ToDouble(dataGridView1.Rows[i].Cells[6].Value);
                    c = b;
                    i = filas;
                }
            }

            for(int i=0;i<filas;i++){
                if (System.Convert.ToDouble(dataGridView1.Rows[i].Cells[6].Value) > 0)
                {
                    //se obtiene el clave ligero
                    if (b >= System.Convert.ToDouble(dataGridView1.Rows[i].Cells[6].Value))
                    {
                        b = System.Convert.ToDouble(dataGridView1.Rows[i].Cells[6].Value);
                        CL = i;
                    }
                    //se obtiene el clave pesado
                    if (c <= System.Convert.ToDouble(dataGridView1.Rows[i].Cells[6].Value))
                    {
                        c = System.Convert.ToDouble(dataGridView1.Rows[i].Cells[6].Value);
                        CP = i;
                    }
                }
            }
            dataGridView1.Rows[CL].DefaultCellStyle.BackColor = Color.CadetBlue;
            dataGridView1.Rows[CP].DefaultCellStyle.BackColor = Color.Coral;
            a=Convert.ToDouble(dataGridView1.Rows[CL].Cells[3].Value);
            d=Convert.ToDouble(dataGridView1.Rows[CP].Cells[3].Value);
            e=Convert.ToDouble(dataGridView1.Rows[CL].Cells[9].Value);
            fmdcp = d;
            fmrcp = c;
            //s e aplica la funcion de femske
            Nmin=fenske(a,b,c,d,e);
            textBox1.Text = Nmin.ToString("#,0.0000");
    
        }
        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex>=0){
                if (e.ColumnIndex == 5 || e.ColumnIndex == 8)//si cambia la volatilidad relativa calcula la volatilidad relativa promedio
                {

                        this.dataGridView1.Rows[e.RowIndex].Cells[9].Value = (System.Convert.ToDouble(this.dataGridView1.Rows[e.RowIndex].Cells[5].Value) +
                            System.Convert.ToDouble(this.dataGridView1.Rows[e.RowIndex].Cells[8].Value)) / 2.0;
                
                }

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            getclaves();//obtiene clves migera y pesada y despliega el numero minimo de etapas
            presionOp();//obtiene la presionde operacion
            underwood();
            kirkbride();
            calorRemovido();
            clearVars();
        }

        //esta funcion regresa todas las variables usadas a cero para cuando se requeira calcular de nuevo
        private void clearVars()
        {
            CL = 0;
            CP = 0;
            fmdcp = 0;
            fmrcp = 0;
            Nmin = 0; 
            Net = 0;
            Ptotal = 0;
            Q = 0;
            Re = 0;
            WFracctotal = 0;
            FFracciontotal = 0;
            Reop = 0;
          //  Nze = 0; 
           // Nza = 0;
        }



        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBox1.Load("untitled.bmp");
            int nze = (int)Math.Round(Nze);
            int nza = (int)Math.Round(Nza);
            int xof = 149;
            int yof = 98;
            int altura = 300;
            int ancho=48;
            int ximp = 15;
            
            
            Pen myPen = new Pen(Color.Black, 5);
            myPen.Width = 3;

            if(nza+nze >0){
                int espacio =(int ) altura / (nza + nze + 1);

                using (Graphics g = Graphics.FromImage(pictureBox1.Image))
                {
               
                    for (int i = 0; i < (nze + nza)+1; i++)
                    {
                        if (i == nze)//esta es la linea de entrada
                        {
                            g.DrawLine(myPen, 50, yof, xof, yof);
                        }
                        else
                        {
                            if (i % 2 == 0)
                            {
                                g.DrawLine(myPen, xof, yof, xof + ancho, yof);
                            }

                            else
                            {
                                g.DrawLine(myPen, xof + ximp, yof, xof + ancho+ximp, yof);
                            }
                        }
                        yof += espacio;

                    }
                }
                pictureBox1.Invalidate();
            }

        }

    }
}
