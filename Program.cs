using WinSCP;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Configuration;
using System.ComponentModel;
using UploadToSFTP.Properties;
using System.Collections.Generic;

// The WinSCP library methods returns nothing, So I had to hard code the error messages.
// This was my very first time I squash bugs in industrial field 

namespace UploadToSFTP {
    static class Program {
        [STAThread]
        public static void Main() {
            
            #region get data from config file
            // get the sink path from the config file
            string sinkPath = ConfigurationManager.AppSettings["To_Path"].ToString();
            // get the source path from the config file
            string sourcePath = ConfigurationManager.AppSettings["From_Path"].ToString();
            // get the username from the config file
            string username = ConfigurationManager.AppSettings["User"].ToString();
            // get the password from the config file
            string password = ConfigurationManager.AppSettings["Password"].ToString();
            // get the session log path from the config file
            string SessionLogPath = ConfigurationManager.AppSettings["SessionLogPath"].ToString();
            // Checking if the session log path has a "\" before adding the file name
            if( SessionLogPath.Substring(SessionLogPath.Length - 1) != @"\" ) {
                SessionLogPath = String.Concat(SessionLogPath, @"\");
            }
            // adding the file name to the path to be created
            SessionLogPath = String.Concat(SessionLogPath, "session.txt");

            // get the Debug log path from the config file
            string DebugLogPath = ConfigurationManager.AppSettings["DebugLogPath"].ToString();
            // Checking if the debug log path has a "\" before adding the file name
            if( DebugLogPath.Substring(DebugLogPath.Length - 1) != @"\" ) {
                DebugLogPath = String.Concat(DebugLogPath, @"\");
            }
            // adding the file name to the path to be created
            DebugLogPath = String.Concat(DebugLogPath, "debug.txt");

            // get the Results path from the config file
            string ResultsPath = ConfigurationManager.AppSettings["ResultsPath"].ToString();
            // Checking if the results log path has a "\" before adding the file name
            if( ResultsPath.Substring(ResultsPath.Length - 1) != @"\" ) {
                ResultsPath = String.Concat(ResultsPath, @"\");
            }
            // adding the file name to the path to be created
            ResultsPath = String.Concat(ResultsPath, "log.txt");

            //get executable path from the config file
            string ExecutePath = ConfigurationManager.AppSettings["ExecutePath"].ToString();

            // get the host Name from the config file
            string hostName = ConfigurationManager.AppSettings["HostName"].ToString();
            #endregion

            #region log texts
            // text to save the messages that will be printed
            string text = String.Empty;
            #endregion

            try {

                #region define session options
                // Defines information to allow an automatic connection and authentication of the session
                SessionOptions sessionOptions = new SessionOptions {
                    // protocol to use for the session. Possible values are Protocol.Sftp (default), Protocol.Scp, Protocol.Ftp and Protocol.Webdav.
                    Protocol = Protocol.Sftp,
                    // Name of the host to connect to. Mandatory property.
                    HostName = hostName,
                    // Username for authentication. Mandatory property.
                    UserName = username,
                    // Password for authentication.
                    Password = password,
                    // Port number to connect to. Keep default 0 to use the default port for the protocol.
                    PortNumber = 00, // enter your port #
                    // Fingerprint of SSH server host key. It makes WinSCP automatically accept host key with the fingerprint. 
                    SshHostKeyFingerprint = "" // you have to enter the fingerprint for your ssh host
                };
                #endregion

                #region upload the file(s)
                // opening a new session
                using( Session session = new Session() ) {
                    // Path to winscp.exe. The default is null, meaning that winscp.exe is looked for in the same directory as this assembly or in an installation folder.
                    //session.ExecutablePath = @ExecutePath;
                    // Path to store assembly debug log to. Default null means, no debug log file is created.
                    session.DebugLogPath = @DebugLogPath;
                    // Path to store session log file to. Default null means, no session log file is created.
                    session.SessionLogPath = @SessionLogPath;
                    // Opens the session.
                    session.Open(sessionOptions);
                    // Defines options for file transfers.
                    TransferOptions transferOptions = new TransferOptions();
                    // As different platforms (operating systems) use different format of text files, many transfer protocols support special mode for transferring text files (called text or ASCII mode).
                    transferOptions.TransferMode = TransferMode.Binary;
                    // Permissions to applied to a remote file (used for uploads only). Use default null to keep default permissions.
                    transferOptions.FilePermissions = null;
                    // set last write time of destination file to that of source file.
                    transferOptions.PreserveTimestamp = false;
                    // Configures automatic resume/transfer to temporary filename.
                    transferOptions.ResumeSupport.State = TransferResumeSupportState.Smart;
                    TransferOperationResult transferResult;
                    // checking if the To_Path ends with a "/"
                    if( sinkPath.Substring(sinkPath.Length - 1) != "/" ) {
                        sinkPath = String.Concat(sinkPath, "/");
                    }
                    // check if the user provided us with the file extension or not
                    if( Path.HasExtension(sourcePath) ) {
                        // Uploads one or more files from local directory to remote directory, remove files from sink? and transfer options
                        transferResult = session.PutFiles(@sourcePath, @sinkPath, false, transferOptions);
                        // Check if files are transferred or not 
                        transferResult.Check();
                    } else {
                        // throw an exception with a message stating the case
                        throw new DirectoryNotFoundException("File extension is not found/valid.");
                    }
                }
                #endregion

                #region Prepare log message
                // if no exceptions were thrown, text will show success of file uploading
                text = String.Format("Success uploading the file from ({0}) to ({1}) at ({2}).", sourcePath, sinkPath, DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss"));
                #endregion
            }

            #region errors that will might occur

                // Checking for the exceptions due to failure of uploading file/session opening 
            catch( UnauthorizedAccessException ex ) {
                text = String.Format("Failed uploading the file from ({0}) to ({1}) at ({2}) due to ({3}). The error message is: ({4}).", sourcePath, sinkPath, DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss"), "Check user rights to edit/save files in this directory. ", ex.Message);
            } catch( DirectoryNotFoundException ex ) {
                if( ex.Message.Contains("Directory folder doesn't exist") ) {
                    text = String.Format("Failed uploading the file from ({0}) to ({1}) at ({2}) due to ({3}).", sourcePath, sinkPath, DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss"), "Remote path is not defined correctly.");
                } else if( ex.Message.Contains("File extension is not found/valid") ) {
                    text = String.Format("Failed uploading the file from ({0}) to ({1}) at ({2}) due to ({3}).", sourcePath, sinkPath, DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss"), "Please check the file extension, it is not found/valid.");
                } else {
                    text = String.Format("Failed uploading the file from ({0}) to ({1}) at ({2}) due to ({3}).", sourcePath, sinkPath, DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss"), "Logs directory is not found.");
                }
            } catch( SessionRemoteException ex ) {
                if( ex.Message.Contains("does not exist") ) {
                    if( ex.Message.Contains("File or folder") ) {
                        text = String.Format("Failed uploading the file from ({0}) to ({1}) at ({2}) due to ({3}).", sourcePath, sinkPath, DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss"), "File or folder does not exist, please check its existence.");
                    } else if( ex.Message.Contains("Host") ) {
                        text = String.Format("Failed uploading the file from ({0}) to ({1}) at ({2}) due to ({3}).", sourcePath, sinkPath, DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss"), "Please check your internet connection.");
                    }
                } else if( ex.Message.Contains("Connection has been unexpectedly closed") ) {
                    text = String.Format("Failed uploading the file from ({0}) to ({1}) at ({2}) due to ({3}).", sourcePath, sinkPath, DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss"), "Please check the username and password used.");
                } else if( ex.Message.Contains("Cannot create remote file") ) {
                    text = String.Format("Failed uploading the file from ({0}) to ({1}) at ({2}) due to ({3}).", sourcePath, sinkPath, DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss"), "Please check the path you are uploading to.");
                } else if( ex.Message.Contains("Can't open file") ) {
                    text = String.Format("Failed uploading the file from ({0}) to ({1}) at ({2}) due to ({3}).", sourcePath, sinkPath, DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss"), "Please check the authorizations on the file you are uploading.");
                }
                if( ex.Message.Contains("timed out") ) {
                    text = String.Format("Failed uploading the file from ({0}) to ({1}) at ({2}) due to ({3}).", sourcePath, sinkPath, DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss"), "Please check the network configurations.");
                }
            } catch( SessionLocalException ex ) {
                text = String.Format("Failed uploading the file from ({0}) to ({1}) at ({2}) due to ({3}). The error message is: ({4}).", sourcePath, sinkPath, DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss"), "Please check WinSCP.exe existance in the setup folder", ex.Message);
            } catch( InvalidOperationException ex ) {
                text = String.Format("Failed uploading the file from ({0}) to ({1}) at ({2}) due to ({3}). The error message is: ({4}).", sourcePath, sinkPath, DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss"), "Session is not opened. ", ex.Message);
            } catch( ArgumentException ex ) {
                text = String.Format("Failed uploading the file from ({0}) to ({1}) at ({2}) due to ({3}). The error message is: ({4}).", sourcePath, sinkPath, DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss"), "Invalid combination of values of TransferOptions properties", ex.Message);
            } catch( TimeoutException ex ) {
                text = String.Format("Failed uploading the file from ({0}) to ({1}) at ({2}) due to ({3}). The error message is: ({4}).", sourcePath, sinkPath, DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss"), "Timeout waiting for winscp.com to respond. ", ex.Message);
            }
            #endregion

            #region Write Log file
            // Writing the messages in the log file
            StreamWriter sw = new StreamWriter(ResultsPath, true);
            // itializes a new instance of the StreamWriter class for the specified stream, using UTF-8 encoding and the default buffer size.
            sw.Write(Environment.NewLine + text + Environment.NewLine);
            // Closes the current StreamWriter object and the underlying stream. 
            sw.Close();
            #endregion

            // System.AppDomain.CurrentDomain.BaseDirectory to get the directory where the current file is installed

        }
    }
}