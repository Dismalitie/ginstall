using ginstall;
using SoftCircuits.IniFileParser;
using System.Net;
using System.Net.NetworkInformation;

double ver = 1.0;

#region funcs

void drawLogo()
{
    Console.WriteLine("█▀▀ █ █▄ █ █▀ ▀█▀ ▄▀█ █   █  ");
    Console.WriteLine("█▄█ █ █ ▀█ ▄█  █  █▀█ █▄▄ █▄▄");
}

bool checkInternet()
{
    try
    {
        using (Ping ping = new Ping())
        {
            PingReply reply = ping.Send("8.8.8.8", 3000); // google public DNS
            return reply.Status == IPStatus.Success;
        }
    }
    catch
    {
        return false;
    }
}

bool check404(string url)
{
    try
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "HEAD"; // Only get headers, not the entire content

        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        {
            // Check if the status code is 404
            return response.StatusCode == HttpStatusCode.NotFound;
        }
    }
    catch (WebException ex)
    {
        // Check if the exception contains a 404 status code
        if (ex.Response is HttpWebResponse response)
        {
            return response.StatusCode == HttpStatusCode.NotFound;
        }
        return false; // Return false for other types of errors
    }
}

void error(string msg)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("/!\\ " + msg);
    Console.ForegroundColor = ConsoleColor.White;
}

void info(string msg)
{
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.WriteLine("(i) " + msg);
    Console.ForegroundColor = ConsoleColor.White;
}

#endregion

if (args.Length == 0)
{
    drawLogo();
    Console.WriteLine("ginstall v1.0");
    Console.WriteLine();

    if (!checkInternet())
    {
        error("No internet! ginstall will not be able to download any packages.");
    }

    info("usage:");
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.WriteLine("ginstall <repo, ex Dismalitie/ginstall>");
    Console.ForegroundColor = ConsoleColor.White;
}
else if (checkInternet())
{
    if (File.Exists(".\\.ginstallCache")) { File.Delete(".\\.ginstallCache"); }

    HttpClient hc = new HttpClient();

    string author = args[0].Split('/')[0];
    string repo = args[0].Split('/')[1];
    string branch = "main";
    if (args[0].Split('/').Length == 3)
    {
        branch = args[0].Split('/')[2];
        if (check404("https://github.com/" + author + "/" + repo + "/tree/" + branch))
        {
            error("The branch you are trying to download from doesn't exist!");
            return;
        }
    }

    if (check404("https://github.com/" + author + "/" + repo))
    {
        error("Repository not found. Check spelling?");
        return;
    }
    
    if (check404("https://raw.githubusercontent.com/" + author + "/" + repo + "/" + branch + "/.ginstall"))
    {
        error("This repository or branch does not support ginstall!");
        return;
    }

    drawLogo();
    Console.WriteLine();

    info("Fetching ginstall manifest...");
    if (File.Exists(".\\.ginstallCache")) { File.Delete(".\\.ginstallCache"); }
    await File.WriteAllTextAsync(".\\.ginstallCache", await hc.GetStringAsync(new Uri("https://raw.githubusercontent.com/" + author + "/" + repo + "/" + branch + "/.ginstall")));

    IniFile manifest = new IniFile();
    manifest.Load(".\\.ginstallCache");

    if (manifest.GetSetting(IniFile.DefaultSectionName, "minVer", ver) > ver)
    {
        error("This repository is using a higher version of ginstall than the currently installed one. Update ginstall to download packages from this repository.");
        return;
    }
    else
    {
        info("Loading package options...");
        Console.WriteLine();
        int iteration = -1;
        List<ginstallPackage> packages = new List<ginstallPackage>();

        foreach (string section in manifest.GetSections())
        {
            if (section.Split(':')[0] == "package")
            {
                iteration++;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[" + iteration + "] " + manifest.GetSetting(section, "name", "Unnamed Package") + " - " + manifest.GetSetting(section, "description", "No description"));
                ginstallPackage pkg = new ginstallPackage()
                {
                    Name = manifest.GetSetting(section, "name", "Unnamed Package"),
                    Description = manifest.GetSetting(section, "description", "No description"),
                    Number = iteration,
                    Files = Array.Empty<string>()
                };
                if (manifest.GetSetting(section, "files", "") != "")
                {
                    pkg.Files = manifest.GetSetting(section, "files", "").Split('|');
                }
                packages.Add(pkg);

                Console.WriteLine("     - Files: " + pkg.Files.Length);
            }
        }

        ginstallPackage selectedPackage = packages.FirstOrDefault();

        void askPackageNumInput() // function becuase requires loop
        {
            Console.WriteLine();
            info("Enter the package number to download:");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("> ");

            string input = Console.ReadLine();
            if (int.TryParse(input, out int result))
            {
                Console.WriteLine();
                selectedPackage = packages[result];
            }
            else
            {
                error("Input is not number.");
                askPackageNumInput();
            }
        }

        askPackageNumInput();
        Console.ForegroundColor = ConsoleColor.White;

        string packageInstallPath = ".\\";

        Console.WriteLine();
        info("Enter the path to download the package to:");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("> ");

        string input = Console.ReadLine();
        packageInstallPath = input;

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Selected package: " + selectedPackage.Name);
        Console.WriteLine("Download path: " + packageInstallPath);
        Console.WriteLine();
        Console.WriteLine("y/n to confirm options: ");
        Console.Write("> ");

        input = Console.ReadLine();
        if (input[0] == 'y')
        {
            Console.WriteLine();
            Console.WriteLine("Starting download of " + selectedPackage.Files.Length + " files.");
            Console.WriteLine();
            WebClient wc = new WebClient();
            foreach (string filepath in selectedPackage.Files)
            {
                Console.WriteLine("Downloading file: " + filepath + " - " + selectedPackage.Files.ToList<string>().IndexOf(filepath)+1 + "/" + selectedPackage.Files.Length);
                try { await wc.DownloadFileTaskAsync(new Uri("https://raw.githubusercontent.com/" + author + "/" + repo + "/" + branch + "/" + filepath), packageInstallPath + Path.GetFileName(filepath)); }
                catch
                {
                    error("Error downloading file. Skipping...");
                    continue;
                }
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Finished downloading all files!");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.White;
            return;
        }

        Console.ForegroundColor = ConsoleColor.White;
        if (File.Exists(".\\.ginstallCache")) { File.Delete(".\\.ginstallCache"); }
    }
}