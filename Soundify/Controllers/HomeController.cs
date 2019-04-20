using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Soundify.Models;
using System.IO;
using System.Web.Hosting;

namespace Soundify.Controllers {
    public class HomeController : Controller {

        //Loads the main home page
        public ActionResult soundify() {
            video x = new video();
            return View(x);
        }


        //Loads the about page
        public ActionResult about() {
            return View();
        }


        //Loads the web player page
        public ActionResult player() {
            video x = new video();
            return View(x);
        }


        //Loads the screenshot clipper page
        public ActionResult clipper() {
            video x = new video();
            return View(x);
        }


        //Loads the file converter page
        public ActionResult converter() {
            video x = new video();
            return View(x);
        }



        //METHODS//

        //Method to collect a URL from a form submission
        public ActionResult collectURL(video x) {
            //Ensure URL is valid
            if (x.validate() == false) return View("soundify", x);

            //Check if the user wants their video trimed, and validate their input
            if (x.startTime != null || x.endTime != null) {
                //Set interval time based off start and end, if the user did not provide a response
                string tempStart = "0", tempEnd = "0";

                if (x.startTime != null || x.startTime != "") {
                    if (x.endTime == null || x.endTime == "") x.endTime = "0";
                    tempStart = x.startTime;
                }

                if (x.endTime != null || x.endTime != "") {
                    if (x.startTime == null || x.startTime == "") x.startTime = "0";
                    tempEnd = x.endTime;
                }

                TimeSpan s, e;
                s = x.textToTimeSpan(tempStart);
                e = x.textToTimeSpan(tempEnd);
                x.interval = e - s;

                //x.interval = TimeSpan.FromMinutes(tempEnd - tempStart).Duration();
                if (x.interval.Equals(0)) x.trimRequest = false;
                else x.trimRequest = true;
            }

            x.clear("~/App_Data/");

            //Otherwise, begin file conversion process
            switch (x.fileType) {
                //Convert to mp3 format
                case ("mp3"): x.toMp3(x.url); return View("soundify", x);

                //Convert to mp4 format
                case ("mp4"): x.toMp4(x.url, false); return View("soundify", x);

                //Convert to wav format
                case ("wav"): x.toWav(x.url); return View("soundify", x);

                //Convert to flv format
                case ("flv"): x.toFlv(x.url); return View("soundify", x);

                //Convert to mov format
                case ("mov"): x.toMov(x.url); return View("converter", x);

                //Convert to wmv format
                case ("wmv"): x.toWmv(x.url); return View("soundify", x);

                default: x.message = "An error has occurred, please try again"; return View("soundify", x);
            }

        }


        //Method to convert a YouTube video to an mp4 video for web playback
        public ActionResult webPlayer(video x) {
            //Ensure URL is valid
            x.fileType = "mp4";
            if (x.validate() == false) return View("player", x);

            else {
                //Convert video to mp4 format for watching in HTML5 video tag
                x.clear("~/App_Data/");
                x.trimRequest = false;
                x.toMp4(x.url, true);
                return View("player", x);
            }
        }


        //Method to convert a YouTube video's frame to a .png image
        public ActionResult screenshot(video x) {
            //Ensure URL is valid
            x.fileType = "jpg";
            if (x.validate() == false) return View("clipper", x);

            //Ensure video time is not null
            if (x.startTime == null || x.startTime == "") {
                x.message = "Requested capture time cannot be empty";
                return View("clipper", x);
            }

            x.interval = x.textToTimeSpan(x.startTime);

            //Clip screenshot image from requested frame
            x.toPng(x.url);
            return View("clipper", x);
        }


        //Method to convert a user's own file
        [HttpPost]
        public ActionResult fileConversion(video x) {
            //Save user's file, it will be deleted in the toNewType function call
            string name = "", path = "";

            if (x.media != null) {
                name = Path.GetFileName(x.media.FileName);
                path = Path.Combine(Server.MapPath("~/App_Data"), name);
                x.media.SaveAs(path);
                x.toNewType(name);
            }

            else {
                x.message = "An error has occurred. Please try again";
            }

            return View("converter", x);
        }

    }
}