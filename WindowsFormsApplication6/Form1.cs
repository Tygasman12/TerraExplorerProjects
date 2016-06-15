using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TerraExplorerX;

namespace WindowsFormsApplication6
{

    public partial class Form1 : Form
    {
        private SGWorld65 globe;
        private SGWorld66 globe1;
        private DateTime savedTime;
        private double MoveDistance, TiltDistance;
        private IPosition65 curPos;
        private ITerrainModel65 MissleObj;
        private ITerrainRegularPolygon65 CircleObj;
        private ITerrainEffect66 explo;
        private _ISGWorld65Events_OnFrameEventHandler myOnFrameEventHandler;


        public Form1()
        {
            InitializeComponent();
            globe = new SGWorld65();
            globe1 = new SGWorld66();

            MoveDistance = 250;
            TiltDistance = 5;

            myOnFrameEventHandler = new _ISGWorld65Events_OnFrameEventHandler(TE_OnFrame);

        }
        private const string EffectXML = @"$$PARTICLE$$UserDefine: 
                     <?xml version='1.0' encoding='UTF-8'?> 
                     <Particle ID='Custom'><ParticleEmitter ID='ring' NumParticles='575' Texture='WhiteSmokeLight.png'>
                    <Emitter Rate='602' Shape='Sphere' SpeedShape='Sphere' Scale='0,0,0' Speed='1,0.51,1' />
                    <Cycle Value='1' />
                    <Sort Value='1' />
                    <Rotation Speed='2' Time='1' Initial='0' />
                    <Render Value='Billboard' />
                    <Gravity Value='0, 0, 0' />
                    <Force Value='0' OverrideRotation='0' />
                    <Position Value='0, 0, 0' />
                    <Life Value='6.25' />
                    <Speed Value='3.28' />
                    <Color Value='20,200,170,160' />
                    <Size Value='4,4' />
                    <Drag Value='0' />
                    <Blend Type='' />
                    <Fade FadeIn='0.13' FadeOut='0.06' MaxFade='0.17' />
                    </ParticleEmitter>
                    <ParticleEmitter ID='ring' NumParticles='572' Texture='Dust.png'>
                    <Emitter Rate='602' Shape='Sphere' SpeedShape='Sphere' Scale='0,0,0' Speed='1,0.76,1' />
                    <Cycle Value='1' />
                    <Sort Value='1' />
                    <Rotation Speed='2' Time='3' Initial='1' />
                    <Render Value='Billboard' />
                    <Gravity Value='0, -1, 0' />
                    <Force Value='0' OverrideRotation='0' />
                    <Position Value='0, 0, 0' />
                    <Life Value='6.25' />
                    <Speed Value='3.28' />
                    <Color Value='20,210,170,145' />
                    <Size Value='4.1,4.1' />
                    <Drag Value='0' />
                    <Blend Type='' />
                    <Fade FadeIn='0.22' FadeOut='0.3' MaxFade='0.14' />
                    </ParticleEmitter>
                    <ParticleEmitter ID='ring' NumParticles='26' Texture='fire.png'>
                    <Emitter Rate='597' Shape='Disc' SpeedShape='Disc' Scale='0,0,0' Speed='1,1,1' />
                    <Cycle Value='1' />
                    <Sort Value='1' />
                    <Render Value='Billboard' />
                    <Gravity Value='0, -1, 0' />
                    <Force Value='0' OverrideRotation='0' />
                    <Position Value='0, 0, 0' />
                    <Life Value='6.25' />
                    <Speed Value='1.56' />
                    <Color Value='20,128,128,255' />
                    <Size Value='3.3,3.3' />
                    <Drag Value='1' />
                    <Blend Type='Add' />
                    <Fade FadeIn='0.2' FadeOut='0' MaxFade='0.8' />
                    </ParticleEmitter>
                    <ParticleEmitter ID='ring' NumParticles='572' Texture='Dust.png'>
                    <Emitter Rate='602' Shape='Cone' SpeedShape='Cone' Scale='0,1,0' Speed='1,1,1' />
                    <Cycle Value='1' />
                    <Sort Value='1' />
                    <Rotation Speed='1' Time='3' Initial='1' />
                    <Render Value='Billboard' />
                    <Gravity Value='0, -1, 0' />
                    <Force Value='0' OverrideRotation='0' />
                    <Position Value='0, 0, 0' />
                    <Life Value='6.25' />
                    <Speed Value='3.75' />
                    <Color Value='20,210,170,145' />
                    <Size Value='4.1,4.1' />
                    <Drag Value='0' />
                    <Blend Type='' />
                    <Fade FadeIn='0.22' FadeOut='0.3' MaxFade='0.13' />
                    </ParticleEmitter>
                    </Particle>";

        private void TE_OnFrame()
        {




            var wpt = globe.Window.CenterPixelToWorld(WorldPointType.WPT_TERRAIN).Position;
            var wpt1 = globe1.Window.CenterPixelToWorld(WorldPointType.WPT_TERRAIN).Position;

            DateTime now = DateTime.Now;
            TimeSpan timeDifference = now.Subtract(savedTime);
            savedTime = now;

            double ftPerSecond = 500.0;

            double distance = ftPerSecond * (timeDifference.Milliseconds / 1000.0);

            IPosition65 newPosition = MissleObj.Position.Move(distance, curPos.Yaw, curPos.Pitch);
            MissleObj.Position = newPosition;
            if (MissleObj.Position.AltitudeType == AltitudeTypeCode.ATC_TERRAIN_ABSOLUTE)
            {
                IPosition65 clone = newPosition.Copy();
                clone = clone.ToRelative(AccuracyLevel.ACCURACY_NORMAL);
                if (clone.Altitude <= 0.0)
                {
                    explo = globe1.Creator.CreateEffect(wpt1, EffectsXML = EffectXML, "", "Explosion");
                    CircleObj = globe.Creator.CreateCircle(wpt, 15.0, LineColor = Color.Red, FillColor = Color.Red, "", "Circle");
                    globe.OnFrame -= myOnFrameEventHandler;
                }
            }

        }

        

       





        private void Forward_(object sender, EventArgs e)
        {
            IPosition65 newPos;
            curPos = globe.Navigate.GetPosition(AltitudeTypeCode.ATC_TERRAIN_ABSOLUTE);
            newPos = curPos.Move(MoveDistance, curPos.Yaw, 0);
            newPos.Yaw = curPos.Yaw;
            newPos.Pitch = curPos.Pitch;
            globe.Navigate.SetPosition(newPos);
        }

        private void Right_(object sender, EventArgs e)
        {
            IPosition65 newPos;
            curPos = globe.Navigate.GetPosition(AltitudeTypeCode.ATC_TERRAIN_ABSOLUTE);
            newPos = curPos.Move(MoveDistance, curPos.Yaw + 90, 0);
            newPos.Yaw = curPos.Yaw;
            newPos.Pitch = curPos.Pitch;
            globe.Navigate.SetPosition(newPos);
        }

        private void Backwards_(object sender, EventArgs e)
        {
            IPosition65 newPos;
            curPos = globe.Navigate.GetPosition(AltitudeTypeCode.ATC_TERRAIN_ABSOLUTE);
            newPos = curPos.Move(-MoveDistance, curPos.Yaw, 0);
            newPos.Yaw = curPos.Yaw;
            newPos.Pitch = curPos.Pitch;
            globe.Navigate.SetPosition(newPos);
        }

        private void Left_(object sender, EventArgs e)
        {
            IPosition65 newPos;
            curPos = globe.Navigate.GetPosition(AltitudeTypeCode.ATC_TERRAIN_ABSOLUTE);
            newPos = curPos.Move(MoveDistance, curPos.Yaw - 90, 0);
            newPos.Yaw = curPos.Yaw;
            newPos.Pitch = curPos.Pitch;
            globe.Navigate.SetPosition(newPos);
        }

        private void TiltUp_(object sender, EventArgs e)
        {
            curPos = globe.Navigate.GetPosition(AltitudeTypeCode.ATC_TERRAIN_ABSOLUTE);
            curPos.Pitch += TiltDistance;
            globe.Navigate.SetPosition(curPos);
        }

        private void TiltRight_(object sender, EventArgs e)
        {
            curPos = globe.Navigate.GetPosition(AltitudeTypeCode.ATC_TERRAIN_ABSOLUTE);
            curPos.Yaw += TiltDistance;
            globe.Navigate.SetPosition(curPos);
        }

        private void TiltDown_(object sender, EventArgs e)
        {
            curPos = globe.Navigate.GetPosition(AltitudeTypeCode.ATC_TERRAIN_ABSOLUTE);
            curPos.Pitch -= TiltDistance;
            globe.Navigate.SetPosition(curPos);
        }

        private void TiltLeft_(object sender, EventArgs e)
        {
            curPos = globe.Navigate.GetPosition(AltitudeTypeCode.ATC_TERRAIN_ABSOLUTE);
            curPos.Yaw -= TiltDistance;
            globe.Navigate.SetPosition(curPos);
        }

        private void Fire_(object sender, EventArgs e)
        {
            curPos = globe.Navigate.GetPosition(AltitudeTypeCode.ATC_TERRAIN_ABSOLUTE);


            var wpt = globe.Window.CenterPixelToWorld().Position;
            var wpt1 = globe1.Window.CenterPixelToWorld().Position;
            string FullPathName;

            FullPathName = @"C:\Program Files (x86)\Skyline\TerraExplorer Pro\Tools\Data-Library\3D-Objects\AirPlanes\f15.xpc";
            MissleObj = globe.Creator.CreateModel(curPos, FullPathName, 0.5, ModelTypeCode.MT_NORMAL, "", "Missle");

            savedTime = DateTime.Now;


            globe.OnFrame += myOnFrameEventHandler;
        }
        public Color LineColor { get; set; }

        public Color FillColor { get; set; }

        public string EffectsXML { get; set; }

       
        private void ZoomOut_(object sender, EventArgs e)
        {
            curPos = globe.Navigate.GetPosition(AltitudeTypeCode.ATC_TERRAIN_ABSOLUTE);
            globe.Navigate.SetPosition(curPos.Move(-MoveDistance, curPos.Yaw, curPos.Pitch));
        }

        private void ZoomIn_(object sender, EventArgs e)
        {
            curPos = globe.Navigate.GetPosition(AltitudeTypeCode.ATC_TERRAIN_ABSOLUTE);
            globe.Navigate.SetPosition(curPos.Move(MoveDistance, curPos.Yaw, curPos.Pitch));
        }

        private void SpeedUp_(object sender, EventArgs e)
        {
            MoveDistance = MoveDistance + 25;
        }

        private void SpeedDown_(object sender, EventArgs e)
        {
            MoveDistance = MoveDistance - 25;

        }


    }
}
