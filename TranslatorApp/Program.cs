using System.Windows.Forms;
using TranslatorApp.Forms;

namespace TranslatorApp;

static class Program
{
    public static MainForm? MainFormRef { get; private set; }

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        MainFormRef = new MainForm();
        Application.Run(MainFormRef);
    }
}
