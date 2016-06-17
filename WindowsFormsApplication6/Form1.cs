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
        private SGWorld66 globe;
        private DateTime savedTime;
        private double MoveDistance, TiltDistance;
        private IPosition66 curPos;
        private _ISGWorld66Events_OnFrameEventHandler myOnFrameEventHandler;
        private List<string> MissileObjects = new List<string>();
        private List<string> MissileIDs = new List<string>();
        private bool iscameraPOV = true;
         
        public Form1()
        {
            InitializeComponent();
            globe = new SGWorld66(); 
     
            MoveDistance = 250; //values for moving the camera
            TiltDistance = 5; // ""

            myOnFrameEventHandler = new _ISGWorld66Events_OnFrameEventHandler(TE_OnFrame);
            globe.OnFrame += myOnFrameEventHandler; 

        }
        private const string EffectXML = @"$$PARTICLE$$UserDefine: 
                     <?xml version='1.0' encoding='UTF-8'?> 
                     <Particle ID='Custom'><ParticleEmitter ID='ring' NumParticles='575' Texture='WhiteSmokeLight.png'>
                    <Emitter Rate='602' Shape='Sphere' SpeedShape='Sphere' Scale='10,10,10' Speed='1,0.51,1' />
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
                    <Size Value='11,11' />
                    <Drag Value='0' />
                    <Blend Type='' />
                    <Fade FadeIn='0.13' FadeOut='0.06' MaxFade='0.17' />
                    </ParticleEmitter>
                    <ParticleEmitter ID='ring' NumParticles='572' Texture='Dust.png'>
                    <Emitter Rate='602' Shape='Sphere' SpeedShape='Sphere' Scale='10,10,10' Speed='1,0.76,1' />
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
                    <Size Value='11.1,11.1' />
                    <Drag Value='0' />
                    <Blend Type='' />
                    <Fade FadeIn='0.22' FadeOut='0.3' MaxFade='0.14' />
                    </ParticleEmitter>
                    <ParticleEmitter ID='ring' NumParticles='26' Texture='fire.png'>
                    <Emitter Rate='597' Shape='Disc' SpeedShape='Disc' Scale='5,5,5' Speed='1,1,1' />
                    <Cycle Value='1' />
                    <Sort Value='1' />
                    <Render Value='Billboard' />
                    <Gravity Value='0, -1, 0' />
                    <Force Value='0' OverrideRotation='0' />
                    <Position Value='0, 0, 0' />
                    <Life Value='6.25' />
                    <Speed Value='1.56' />
                    <Color Value='20,128,128,255' />
                    <Size Value='10.3,10.3' />
                    <Drag Value='1' />
                    <Blend Type='Add' />
                    <Fade FadeIn='0.2' FadeOut='0' MaxFade='0.8' />
                    </ParticleEmitter>
                    </Particle>";

        private void TE_OnFrame()
        {

            DateTime now = DateTime.Now; //gets the time at the moment its executed
            TimeSpan timeDifference = now.Subtract(savedTime);
            savedTime = now;

            double ftPerSecond = 500.0;
            double distance = ftPerSecond * (timeDifference.Milliseconds / 1000.0); 
            for (int i = 0; i < MissileIDs.Count; i++)
            {
                IPosition66 newPosition = null;
                ITerrainModel66 MissleObj = (ITerrainModel66) globe.Creator.GetObject(MissileIDs[i]);
                double distanceTo = System.Convert.ToDouble(MissleObj.ClientData["Distancetoground"]); //gets the distance from the missile to the ground
                if (distanceTo <= distance)
                {
                    newPosition = MissleObj.Position.Move(distanceTo, MissleObj.Position.Yaw, MissleObj.Position.Pitch); //moves the missile x amount of feet towards target
                    newPosition.AltitudeType = AltitudeTypeCode.ATC_ON_TERRAIN; //
                    newPosition.Altitude = 0;
                    ITerrainEffect66 explo = globe.Creator.CreateEffect(newPosition, EffectsXML = EffectXML, "", "Explosion"); //creates an explosion effect at the point where the missile hit the terrain
                    MissileObjects.Add(explo.ID); //adds the effects ID to an array
                    ITerrainRegularPolygon66 CircleObj = globe.Creator.CreateCircle(newPosition, 15.0, LineColor = Color.Red, FillColor = Color.Red, "", "Circle"); //creates a red circle at the point where the missile hit the terrain
                    MissileObjects.Add(CircleObj.ID); //adds the circles ID to an array 
                    globe.ProjectTree.DeleteItem(MissileIDs[i]); //deletes the missile when it has hit the terrain
                    MissileIDs.RemoveAt(i);
                    i--;
                    
                    
                }
                else
                {
                    newPosition = MissleObj.Position.Move(distance, MissleObj.Position.Yaw, MissleObj.Position.Pitch); //keeps the missile moving till it hits the terrain
                    MissleObj.Position = newPosition;
                    if (MissleObj.Position.Roll < 359.0) //makes the missile spin as it moves towards the desired target
                    {
                        MissleObj.Position.Roll += 9.0; //roll value changes spin speed
                    }
                    else
                    {

                        MissleObj.Position.Roll = 0.0;
                    }
               
                    MissleObj.ClientData["Distancetoground"] = (distanceTo - distance).ToString();
                }

            }
            if (MissileIDs.Count > 0)
            {

                if (!iscameraPOV)
                {
                    globe.Navigate.JumpTo(MissileIDs[MissileIDs.Count - 1]);
                }
               
            }
        }

        private void Fire_(object sender, EventArgs e)
        {
            var wpt = globe.Window.CenterPixelToWorld(WorldPointType.WPT_TERRAIN);// gets the position of the camera based on the center pixel of when it was executed
            
            curPos = globe.Navigate.GetPosition(AltitudeTypeCode.ATC_TERRAIN_ABSOLUTE);//gets position of camera
            double distanceTo = curPos.DistanceTo(wpt.Position);

            string FullPathName;
            FullPathName = @"C:\Users\Skyline\Desktop\model.dae";
            ITerrainModel66 MissleObj = globe.Creator.CreateModel(curPos, FullPathName, 5, ModelTypeCode.MT_NORMAL, "", "Missle"); //creates missile model
            MissleObj.Attachment.AutoDetach = false; 
            MissleObj.Action.Code = ActionCode.AC_FOLLOWBEHIND; //when you want to follow missile, it follows from behind
            MissleObj.ClientData["Distancetoground"] = distanceTo.ToString();
            MissileIDs.Add(MissleObj.ID); //adds missile ID to an array
            savedTime = DateTime.Now;

        }
        private void Remove_(object sender, EventArgs e)
        {
            foreach (var item in MissileObjects)
            {
                globe.ProjectTree.DeleteItem(item); //deletes each item in the project tree
            }
            MissileObjects.Clear();

           
        }
        private void Forward_(object sender, EventArgs e)
        {
            IPosition66 newPos;
            curPos = globe.Navigate.GetPosition(AltitudeTypeCode.ATC_TERRAIN_ABSOLUTE);
            newPos = curPos.Move(MoveDistance, curPos.Yaw, 0);
            newPos.Yaw = curPos.Yaw;
            newPos.Pitch = curPos.Pitch;
            globe.Navigate.SetPosition(newPos);
        }

        private void Right_(object sender, EventArgs e)
        {
            IPosition66 newPos;
            curPos = globe.Navigate.GetPosition(AltitudeTypeCode.ATC_TERRAIN_ABSOLUTE);
            newPos = curPos.Move(MoveDistance, curPos.Yaw + 90, 0);
            newPos.Yaw = curPos.Yaw;
            newPos.Pitch = curPos.Pitch;
            globe.Navigate.SetPosition(newPos);
        }

        private void Backwards_(object sender, EventArgs e)
        {
            IPosition66 newPos;
            curPos = globe.Navigate.GetPosition(AltitudeTypeCode.ATC_TERRAIN_ABSOLUTE);
            newPos = curPos.Move(-MoveDistance, curPos.Yaw, 0);
            newPos.Yaw = curPos.Yaw;
            newPos.Pitch = curPos.Pitch;
            globe.Navigate.SetPosition(newPos);
        }

        private void Left_(object sender, EventArgs e)
        {
            IPosition66 newPos;
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

        private void  POVChanged_(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                iscameraPOV = false;
            }
            else
            {
                iscameraPOV = true;
                if (curPos != null)
                {
                    globe.Navigate.SetPosition(curPos);
                }
            }
        }

        

    }
}