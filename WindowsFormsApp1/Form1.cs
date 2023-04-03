using System;
using System.Drawing;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.MapProviders;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace WindowsFormsApp1
{
    public class Unit
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }

        public Unit(int id, string name, double lat, double lon)
        {
            Id = id;
            Name = name;
            Lat = lat;
            Lon = lon;
        }
    }

    public class FuctionUnits
    {
        private SqlConnection connection;

        public void openConnectionDB(string connectionString)
        {
            connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
            }
            catch
            {
                MessageBox.Show("Не удалось подключиться к БД!");
            }
        }

        public bool isConnectionOpen()
        {
            if (connection != null && connection.State == System.Data.ConnectionState.Open)
                return true;
            else
                return false;
        }

        public void closeConnectionDB()
        {
            connection.Close();
        }

        public List<Unit> CreateUnits()
        {
            List<Unit> units = new List<Unit>();

            if (isConnectionOpen())
            {
                string query = "SELECT * FROM units";
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    units.Add(new Unit(Convert.ToInt32(reader["id"]), Convert.ToString(reader["name"]),
                        Convert.ToDouble(reader["lat"]), Convert.ToDouble(reader["lon"])
                        ));
                }
                reader.Close();
            }
            return units;
        }

        public void changePosition(GMapMarker ClickMarker, PointLatLng pointClickMarker)
        {
            if (isConnectionOpen())
            {
                string[] id = ClickMarker.ToolTipText.Split('.');
                string query = "UPDATE units SET lat=" + Convert.ToString(pointClickMarker.Lat).Replace(',', '.')
                        + ", lon=" + Convert.ToString(pointClickMarker.Lng).Replace(',', '.') + " WHERE id=" + id[0];
                SqlCommand command = new SqlCommand(query, connection);
                command.ExecuteNonQuery();
            }
        }
    }

    public partial class Form1 : Form
    {
        GMapMarker ClickMarker = null;
        PointLatLng pointClickMarker;
        Point MouseDownPoint;
        bool isMouseDown = false;

        FuctionUnits units = new FuctionUnits();

        public Form1()
        {
            InitializeComponent();
        }

        public GMapOverlay CreateMarkers(List<Unit> listUnits)
        {
            GMapOverlay markers = new GMapOverlay("markers");
            foreach (var unit in listUnits)
            {
                GMapMarker marker = new GMarkerGoogle(
                                    new PointLatLng(unit.Lat, unit.Lon),
                                    GMarkerGoogleType.orange);
                marker.ToolTipText = Convert.ToString(unit.Id) + ". " + unit.Name;
                markers.Markers.Add(marker);
            }

            return markers;
        }

        private void gMapControl1_Load(object sender, EventArgs e)
        {
            gMapControl1.MapProvider = GoogleMapProvider.Instance;
            GMaps.Instance.Mode = AccessMode.ServerOnly;
            gMapControl1.MinZoom = 2; 
            gMapControl1.MaxZoom = 16; 
            gMapControl1.Zoom = 13; 
            gMapControl1.Position = new GMap.NET.PointLatLng(55.030204, 82.92043);
            gMapControl1.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter; 
            gMapControl1.CanDragMap = true; 
            gMapControl1.DragButton = MouseButtons.Right; 
            gMapControl1.ShowCenter = false; 
            gMapControl1.ShowTileGridLines = false;

            string connectionString = @"Data Source=LAPTOP-S6SIUI0Q; Initial Catalog=dbunits;Integrated Security=True";
            units.openConnectionDB(connectionString);
            gMapControl1.Overlays.Add(CreateMarkers(units.CreateUnits()));
        }

        private void gMapControl1_OnMarkerEnter(GMapMarker item)
        {
            if(ClickMarker == null)
                ClickMarker = item;
        }

        private void gMapControl1_OnMarkerLeave(GMapMarker item)
        {
            if (!isMouseDown)
            {
                ClickMarker = null;
            }
            
        }

        private void gMapControl1_MouseDown(object sender, MouseEventArgs e)
        {
            isMouseDown = true;
            MouseDownPoint = new Point(e.Location.X, e.Location.Y);
        }

        private void gMapControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (ClickMarker != null && isMouseDown && e.Button == MouseButtons.Left)
            {
                pointClickMarker = gMapControl1.FromLocalToLatLng(e.Location.X, e.Location.Y);
                ClickMarker.Position = new PointLatLng(pointClickMarker.Lat, pointClickMarker.Lng);
            }
        }

        private void gMapControl1_MouseUp(object sender, MouseEventArgs e)
        {
            if (ClickMarker != null)
            {
                isMouseDown = false;
                units.changePosition(ClickMarker, pointClickMarker);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            units.closeConnectionDB();
        }
    }
}
