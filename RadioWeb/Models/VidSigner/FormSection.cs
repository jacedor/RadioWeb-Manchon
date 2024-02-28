using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RadioWeb.Models.VidSigner
{
    public class FormSection
    {        
         
        public List<FormGroup> Groups;

    }

    public class FormGroup
    {
        public List<FormCheckBoxDTO> CheckBoxes;
        public List<FormRadioButtonDTO> RadioButtons;
        public List<FormTextBoxDTO> TextBoxes;
        public FormTitleDTO Title;

       
    }
       

    public class FormConditionDTO
    {
        public string Id;
        public int Result;      
    }

    public class FormPositionDTO
    {
        public string Anchor;
        public int Page;
        public int PosX;
        public int PosY;
        public int SizeX;        
    }
    public class FormTitleDTO
    {
        public FormPositionDTO Position;
        public string Text;        
    }

    public class FormCheckBoxDTO
    {
        public FormConditionDTO Condition;
        public string Id;
        public bool Response;
        public FormTitleDTO Title;      
    }

    public class FormRadioButtonDTO
    {
        public List<FormRadioButtonChoiceDTO> Choices;
        public FormConditionDTO Condition;
        public string Id;
        public int SelectedChoice;
        public FormTitleDTO Title;

     
    }

    public class FormTextBoxDTO
    {
        public FormConditionDTO Condition;
        public string Id;
        public int MaxLines;
        public FormTitleDTO Response;
        public FormTitleDTO Title;

        
    }

    public class FormRadioButtonChoiceDTO
    {
        public FormTitleDTO Title;

        
    }
}