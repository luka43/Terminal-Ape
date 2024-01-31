using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalApe.UI.Services;

public class AllControls
{
    
    private MainAppForm mainAppForm;
    private Dictionary<string, Control> controlDictionary;

    public AllControls(MainAppForm form)
    {
        mainAppForm = form;
        InitializeControlDictionary();
    }

    private void InitializeControlDictionary()
    {
        controlDictionary = new Dictionary<string, Control>();
        GetAllControls(mainAppForm);
    }

    public void GetAllControls(Control control)
    {
        foreach (Control childControl in control.Controls)
        {
            controlDictionary[childControl.Name] = childControl;

            if (childControl.HasChildren)
            {
                GetAllControls(childControl);
            }
        }
    }

    public Control GetControlByName(string name)
    {
        if (controlDictionary.TryGetValue(name, out var control))
        {
            return control;
        }

        return null; // Control not found
    }
}
