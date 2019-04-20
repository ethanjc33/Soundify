using System;
using System.IO;
using System.ComponentModel.DataAnnotations;
using MediaToolkit; //.NET library to convert audio and video files
using MediaToolkit.Model;
using VideoLibrary; //Third-party package to download YouTube videos (HIG fork dependency)
using System.Web.Hosting;
using MediaToolkit.Options;
using System.Linq;
using System.Web;

namespace Soundify.Models {
    public class video {

        //MEMBERS//

        //Holds the URL, collected via our website form
        //[Required(ErrorMessage = "URL cannot be empty")]
        public string url { get; set; }

        //Holds the path to our new downloaded file
        public string fileLocation { get; set; }

        //Holds the requested file type extension
        public string fileType { get; set; }

        //Holds the requested output trimmed start time
        public string startTime { get; set; }

        //Holds the requested output trimmed end time
        public string endTime { get; set; }

        //Holds the requested output trimmed interval time
        public TimeSpan interval { get; set; }

        //Holds whether or not the user wants their video trimmed
        public bool trimRequest { get; set; }

        //Records an error or success message at the critical points
        public string message { get; set; }

        //Holds a personal file to be converted by the user
        public HttpPostedFileBase media { get; set; }



        //METHODS//

        //Converts a YouTube video to an mp3 file
        public void toMp3(string link) {
            //This uses third-party packages to get source video from YouTube
            var sourceVid = YouTube.Default.GetVideo(link);
            string newName = sourceVid.Title + ".mp3";
            string path = HostingEnvironment.MapPath("~/App_Data/");
            string type = "audio/mpeg3";

            //Saves video as an .mp4 file
            File.WriteAllBytes(sourceVid.FullName, sourceVid.GetBytes());

            //Create our new files for .mp3 conversion
            var inputFile = new MediaFile { Filename = sourceVid.FullName };
            var outFile = new MediaFile { Filename = $"{sourceVid.Title}.mp3" };

            //MediaToolkit engine helps us convert the file from .mp4 to .mp3
            using (var e = new Engine()) {
                e.GetMetadata(inputFile);

                //Trim output file if requested
                if (trimRequest == true) {
                    TimeSpan tempStart, tempEnd;
                    tempStart = textToTimeSpan(startTime);
                    tempEnd = textToTimeSpan(endTime);

                    if (interval > inputFile.Metadata.Duration) {
                        message = "Trim duration cannot exceed video length";
                        return;
                    }

                    if (tempStart > inputFile.Metadata.Duration ||
                        tempEnd > inputFile.Metadata.Duration) {
                        message = "Start / end time cannot exceed video length";
                        return;
                    }

                    var options = new ConversionOptions();
                    options.CutMedia(tempStart, interval);
                    e.Convert(inputFile, outFile, options);
                }

                else e.Convert(inputFile, outFile);
            }

            //Clean up and delete the source .mp4 file, prompt user to save new file
            File.Delete(sourceVid.FullName);
            File.Move(outFile.Filename, path + newName);
            dialog(type, path, newName);
            File.Delete(path + sourceVid.FullName);

            return;
        }


        //Convert YouTube video to .flv file
        public void toFlv(string link) {
            //Grab source video, establish paths
            var sourceVid = YouTube.Default.GetVideo(link);
            string newName = sourceVid.Title + ".flv";
            string path = HostingEnvironment.MapPath("~/App_Data/");
            string type = "video/x-flv";

            //Convert from YouTube video to .mp4 file
            File.WriteAllBytes(sourceVid.FullName, sourceVid.GetBytes());
            var inputFile = new MediaFile { Filename = sourceVid.FullName };
            var outFile = new MediaFile { Filename = $"{sourceVid.Title}.flv" };

            //MediaToolkit engine
            using (var e = new Engine()) {
                e.GetMetadata(inputFile);

                //Trim output file if requested
                if (trimRequest == true) {
                    TimeSpan tempStart, tempEnd;
                    tempStart = textToTimeSpan(startTime);
                    tempEnd = textToTimeSpan(endTime);

                    if (interval > inputFile.Metadata.Duration) {
                        message = "Trim duration cannot exceed video length";
                        return;
                    }

                    if (tempStart > inputFile.Metadata.Duration ||
                        tempEnd > inputFile.Metadata.Duration) {
                        message = "Start / end time cannot exceed video length";
                        return;
                    }

                    var options = new ConversionOptions();
                    options.CutMedia(tempStart, interval);
                    e.Convert(inputFile, outFile, options);
                }

                else e.Convert(inputFile, outFile);
            }

            //Create final file that the user will be prompted to download
            File.Delete(sourceVid.FullName);
            File.Move(outFile.Filename, path + newName);
            dialog(type, path, newName);
            File.Delete(path + sourceVid.FullName);

            return;
        }


        //Convert YouTube video to .mp4 file
        public void toMp4(string link, bool playback) {
            //This uses third-party packages to get source video from YouTube
            var sourceVid = YouTube.Default.GetVideo(link);
            string path = HostingEnvironment.MapPath("~/App_Data/");
            string type = "video/mpeg";

            //Convert from YouTube video to .mp4 file
            File.WriteAllBytes(sourceVid.FullName, sourceVid.GetBytes());
            var inputFile = new MediaFile { Filename = sourceVid.FullName };
            var outFile = new MediaFile { Filename = $"{sourceVid.Title}.mp4" };

            //Check if this video was converted for playback purposes
            if (playback == true) {
                clear("~/Videos/");
                string newPath = HostingEnvironment.MapPath("~/Videos/");
                File.Move(inputFile.Filename, newPath + "vid.mp4");
                fileLocation = inputFile.Filename;
                fileType = sourceVid.Title;
                return;
            }

            //Only handle conversion process if user wants video trimmed
            if (trimRequest == true) {

                //Feature not supported message
                message = "The trim feature is currently not supported for mp4 file types";
                File.Delete(sourceVid.FullName);
                return;

                /*
                using (var e = new Engine()) {
                    e.GetMetadata(inputFile);

                    TimeSpan tempStart, tempEnd;
                    tempStart = textToTimeSpan(startTime);
                    tempEnd = textToTimeSpan(endTime);

                    if (interval > inputFile.Metadata.Duration) {
                        message = "Trim duration cannot exceed video length";
                        return;
                    }

                    if (tempStart > inputFile.Metadata.Duration ||
                        tempEnd > inputFile.Metadata.Duration) {
                        message = "Start / end time cannot exceed video length";
                        return;
                    }

                    //Note: Conversion does not seem to be supported in this case
                    //Likely due to ffmpeg checking that we're converting an mp4 file to an mp4 file 
                    var options = new ConversionOptions();
                    options.CutMedia(tempStart, interval);
                    e.Convert(inputFile, outFile, options);


                    //Create final file that the user will be prompted to download
                    File.Move(inputFile.Filename, path + outFile.Filename);
                    dialog(type, path, outFile.Filename);
                    return;
                }*/
            }

            //Create final file that the user will be prompted to download
            File.Move(inputFile.Filename, path + inputFile.Filename);

            dialog(type, path, inputFile.Filename);

            return;
        }


        //Converts YouTube video to wav file
        public void toWav(string link) {
            //Grab source video, establish paths
            var sourceVid = YouTube.Default.GetVideo(link);
            string newName = sourceVid.Title + ".wav";
            string path = HostingEnvironment.MapPath("~/App_Data/");
            string type = "audio/wav";

            //Convert from YouTube video to .mp4 file
            File.WriteAllBytes(sourceVid.FullName, sourceVid.GetBytes());

            //Create new files for conversion from .mp4 to .wav
            var inputFile = new MediaFile { Filename = sourceVid.FullName };
            var outFile = new MediaFile { Filename = $"{sourceVid.Title}.wav" };

            //MediaToolkit engine helps us convert the file from .mp4 to .wav
            using (var e = new Engine()) {
                e.GetMetadata(inputFile);

                //Trim output file if requested
                if (trimRequest == true) {
                    TimeSpan tempStart, tempEnd;
                    tempStart = textToTimeSpan(startTime);
                    tempEnd = textToTimeSpan(endTime);

                    if (interval > inputFile.Metadata.Duration) {
                        message = "Trim duration cannot exceed video length";
                        return;
                    }

                    if (tempStart > inputFile.Metadata.Duration ||
                        tempEnd > inputFile.Metadata.Duration) {
                        message = "Start / end time cannot exceed video length";
                        return;
                    }

                    var options = new ConversionOptions();
                    options.CutMedia(tempStart, interval);
                    e.Convert(inputFile, outFile, options);
                }

                else e.Convert(inputFile, outFile);
            }

            //Clean up and delete the source .mp4 file, prompt user to save new file
            File.Delete(sourceVid.FullName);
            File.Move(outFile.Filename, path + newName);
            dialog(type, path, newName);
            File.Delete(path + sourceVid.FullName);

            return;
        }


        //Converts YouTube video to mov file
        public void toMov(string link) {
            //Grab source video, establish paths
            var sourceVid = YouTube.Default.GetVideo(link);
            string newName = sourceVid.Title + ".mov";
            string path = HostingEnvironment.MapPath("~/App_Data/");
            string type = "video/quicktime";

            //Convert from YouTube video to .mp4 file
            File.WriteAllBytes(sourceVid.FullName, sourceVid.GetBytes());
            var inputFile = new MediaFile { Filename = sourceVid.FullName };
            var outFile = new MediaFile { Filename = $"{sourceVid.Title}.mov" };

            //MediaToolkit engine
            using (var e = new Engine()) {
                e.GetMetadata(inputFile);

                //Trim output file if requested
                if (trimRequest == true) {
                    TimeSpan tempStart, tempEnd;
                    tempStart = textToTimeSpan(startTime);
                    tempEnd = textToTimeSpan(endTime);

                    if (interval > inputFile.Metadata.Duration) {
                        message = "Trim duration cannot exceed video length";
                        return;
                    }

                    if (tempStart > inputFile.Metadata.Duration ||
                        tempEnd > inputFile.Metadata.Duration) {
                        message = "Start / end time cannot exceed video length";
                        return;
                    }

                    var options = new ConversionOptions();
                    options.CutMedia(tempStart, interval);
                    e.Convert(inputFile, outFile, options);
                }

                else e.Convert(inputFile, outFile);
            }

            //Create final file that the user will be prompted to download
            File.Delete(sourceVid.FullName);
            File.Move(outFile.Filename, path + newName);
            dialog(type, path, newName);
            File.Delete(path + sourceVid.FullName);

            return;
        }


        //Converts YouTube video to wmv file
        public void toWmv(string link) {
            //Grab source video, establish paths
            var sourceVid = YouTube.Default.GetVideo(link);
            string newName = sourceVid.Title + ".wmv";
            string path = HostingEnvironment.MapPath("~/App_Data/");
            string type = "video/x-mswmv";

            //Convert from YouTube video to .mp4 file
            File.WriteAllBytes(sourceVid.FullName, sourceVid.GetBytes());
            var inputFile = new MediaFile { Filename = sourceVid.FullName };
            var outFile = new MediaFile { Filename = $"{sourceVid.Title}.wmv" };

            //MediaToolkit engine
            using (var e = new Engine()) {
                e.GetMetadata(inputFile);

                //Trim output file if requested
                if (trimRequest == true) {
                    TimeSpan tempStart, tempEnd;
                    tempStart = textToTimeSpan(startTime);
                    tempEnd = textToTimeSpan(endTime);

                    if (interval > inputFile.Metadata.Duration) {
                        message = "Trim duration cannot exceed video length";
                        return;
                    }

                    if (tempStart > inputFile.Metadata.Duration ||
                        tempEnd > inputFile.Metadata.Duration) {
                        message = "Start / end time cannot exceed video length";
                        return;
                    }

                    var options = new ConversionOptions();
                    options.CutMedia(tempStart, interval);
                    e.Convert(inputFile, outFile, options);
                }

                else e.Convert(inputFile, outFile);
            }

            //Create final file that the user will be prompted to download
            File.Delete(sourceVid.FullName);
            File.Move(outFile.Filename, path + newName);
            dialog(type, path, newName);
            File.Delete(path + sourceVid.FullName);

            return;
        }


        //Converts YouTube video frame to png image
        public void toPng(string link) {
            //Grab source video, establish paths
            var sourceVid = YouTube.Default.GetVideo(link);
            string newName = sourceVid.Title + ".png";
            string path = HostingEnvironment.MapPath("~/App_Data/");
            string type = "image/png";

            //Convert from YouTube video to .mp4 file
            File.WriteAllBytes(sourceVid.FullName, sourceVid.GetBytes());

            //Create new files for screenshot capture
            var inputFile = new MediaFile { Filename = sourceVid.FullName };
            var outFile = new MediaFile { Filename = $"{sourceVid.Title}.png" };

            //MediaToolkit engine helps us clip the requested frame
            using (var e = new Engine()) {
                e.GetMetadata(inputFile);

                //Ensure requested time is within bounds
                if (interval > inputFile.Metadata.Duration) {
                    message = "Requested capture time cannot exceed video duration";
                    return;
                }

                var options = new ConversionOptions { Seek = interval };
                e.GetThumbnail(inputFile, outFile, options);
            }

            //Clean up and delete the source .mp4 file, prompt user to save new file
            File.Delete(sourceVid.FullName);
            File.Move(outFile.Filename, path + newName);
            dialog(type, path, newName);
            File.Delete(path + sourceVid.FullName);
        }


        //Converts a personal Media File to another file type
        public void toNewType(string name) {
            //Note: Validation for audio and video files only occurs on the client side
            int extSpot = name.LastIndexOf('.');
            string ext = name.Substring(extSpot + 1);
            string newName = name.Substring(0, name.Length - 4) + "." + fileType;
            string path = HostingEnvironment.MapPath("~/App_Data/");
            string type = "";

            //Check if the file actually needs converting
            if (ext == fileType) {
                message = "This file is already of the requested type";
                return;
            }

            //Set Http type response
            switch (fileType) {
                case "mp3": type = "audio/mpeg"; break;
                case "wav": type = "audio/wav"; break;
                case "mp4": type = "video/mpeg"; break;
                case "flv": type = "video/x-flv"; break;
                case "mov": type = "video/quicktime"; break;
                case "wmv": type = "video/x-mswmv"; break;
                default: message = "An error occurred. Please try again."; return;
            }

            //File is converted by saving a copy of the user's requested file,
            //changing its extension after validating that we can,
            //and finally returning the new converted copy back to the user
            File.Move(path + name, path + newName);

            //Return converted file back to the user
            dialog(type, path, newName);
            File.Delete(path + newName);
            return;
        }



        //HELPER FUNCTIONS//

        //User save dialog method
        public void dialog(string conType, string path, string name) {
            //Open dialog so that the user can save their file
            System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;
            response.ClearContent();
            response.Clear();
            response.ContentType = conType;
            response.AddHeader("Content-Disposition", "attachment; filename=" + name + ";");
            response.TransmitFile(path + name);
            response.Flush();

            message = null;
            File.Delete(path + name);

            return;
        }


        //YouTube video validation check
        public bool validate() {
            //If the URL is null
            if (url == null) {
                message = "URL cannot be empty";
                return false;
            }

            //If the URL is not a YouTube video, set error message and do not convert
            if (!url.StartsWith("https://www.youtube.com/watch?v=")) {
                message = "URL must come from a public YouTube video";
                return false;
            }

            //Check if the user has not selected a desired file output type
            if (fileType == null) {
                message = "You must select a desired file type";
                return false;
            }

            //Otherwise, video is valid for conversion
            return true;
        }


        //Clear contents of App_Data, where our videos will be stored
        public void clear(string folder) {
            //Clear contents of a directory, solution taken from...
            //Source: https://stackoverflow.com/questions/1288718/how-to-delete-all-files-and-folders-in-a-directory
            DirectoryInfo d = new DirectoryInfo(HostingEnvironment.MapPath(folder));
            foreach (FileInfo file in d.GetFiles()) file.Delete();
            foreach (DirectoryInfo dir in d.GetDirectories()) dir.Delete(true);
            return;
        }


        //Convert start and end time to 
        public TimeSpan textToTimeSpan(string inputTime) {
            if (inputTime != null && inputTime != "" && inputTime != "0") {
                //Set minutes value
                string hold = "";
                int i = 0;
                while (inputTime.ElementAt(i) != ':') {
                    hold += inputTime.ElementAt(i);
                    i++;
                }

                int minutes = 0;
                int.TryParse(hold, out minutes);

                //Set seconds value
                hold = "";
                i++;
                while (i < inputTime.Length) {
                    hold += inputTime.ElementAt(i);
                    i++;
                }

                int seconds = 0;
                int.TryParse(hold, out seconds);

                //Set TimeSpan object
                TimeSpan requestedTime = new TimeSpan(0, minutes, seconds);

                return requestedTime;
            }

            else return new TimeSpan { };
        }


    }

}