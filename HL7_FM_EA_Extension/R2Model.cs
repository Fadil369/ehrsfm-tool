﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EA;

namespace HL7_FM_EA_Extension
{
    /**
     * This model represents the EHR-S FM R2 UML Profile
     * and wraps all the EA Model classes
     */
    public class R2Model
    {
        // add factory method to create correct model class based on the EA.Element
        public static R2ModelElement Create(EA.Repository repository, EA.Element element)
        {
            switch (element.Stereotype)
            {
                case R2Const.ST_SECTION:
                    return new R2Section(getModelElementPath(repository, element), element);
                case R2Const.ST_HEADER:
                case R2Const.ST_FUNCTION:
                    return new R2Function(getModelElementPath(repository, element), element);
                case R2Const.ST_CRITERION:
                    return new R2Criterion(getModelElementPath(repository, element), element);
                case R2Const.ST_COMPILERINSTRUCTION:
                    EA.Connector generalization = element.Connectors.Cast<EA.Connector>().SingleOrDefault(t => "Generalization".Equals(t.Type));
                    EA.Element baseElement = repository.GetElementByID(generalization.SupplierID);
                    switch(baseElement.Stereotype)
                    {
                        case R2Const.ST_SECTION:
                            return new R2SectionCI(getModelElementPath(repository, baseElement), element, baseElement);
                        case R2Const.ST_HEADER:
                        case R2Const.ST_FUNCTION:
                            return new R2FunctionCI(getModelElementPath(repository, baseElement), element, baseElement);
                        case R2Const.ST_CRITERION:
                            return new R2CriterionCI(getModelElementPath(repository, baseElement), element, baseElement);
                        default:
                            return null;
                    }
                default:
                    return null;
            }
        }

        /**
         * Create a string containing the element path joined with '/' up to the <HL7-FM> stereotyped package.
         * This is used for as title for Section/Header/Function/Criteria Forms.
         */
        public static string getModelElementPath(EA.Repository repository, EA.Element element)
        {
            List<string> path = new List<string>();
            EA.Element el = element;
            while (el != null && !R2Const.ST_FM.Equals(el.Stereotype))
            {
                path.Insert(0, el.Name);
                if (el.ParentID == 0)
                {
                    // Don't include package name, that is obvious from the Header/Function Code.
                    // el = Repository.GetPackageByID(el.PackageID).Element;
                    break;
                }
                else
                {
                    el = repository.GetElementByID(el.ParentID);
                }
            }
            return string.Join(" / ", path.ToArray());
        }

        public static Dictionary<string,string> splitNotes(string notes)
        {
            System.Text.RegularExpressions.Regex regex2 = new System.Text.RegularExpressions.Regex(@"\$([A-Z]{2})\$");
            string[] parts = regex2.Split(notes, 10);
            Dictionary<string, string> dict = new Dictionary<string, string>();
            for (int i = 1; i < parts.Length; i += 2)
            {
                dict[parts[i]] = (parts.Length > i + 1) ? parts[i + 1] : "";
            }
            return dict;
        }
    }

    public class R2Const
    {
        public const string ST_FM = "HL7-FM";
        public const string ST_FM_PROFILEDEFINITION = "HL7-FM-ProfileDefinition";
        public const string ST_COMPILERINSTRUCTION = "CI";
        public const string ST_BASEMODEL = "use";
        public const string ST_TARGETPROFILE = "create";
        public const string ST_FM_PROFILE = "HL7-FM-Profile";
        public const string ST_SECTION = "Section";
        public const string ST_HEADER = "Header";
        public const string ST_FUNCTION = "Function";
        public const string ST_CRITERION = "Criteria"; // TODO!
        public const string ST_CONSEQUENCELINK = "ConsequenceLink";
        public const string ST_SEEALSO = "SeeAlso";

        public enum ProfileTypes
        {
            Companion,
            Domain,
            Realm,
            Derived,
            Combined
        };

        public enum Qualifier
        {
            DEP,    // Deprecate
            D       // Delete
        };

        public enum Priority
        {
            EN,     // Essential Now
            EF      // Essential Future
        };
    }

    public interface CompilerInstruction
    {
        // string Priority;
        // string ChangeNote;
        // string Qualifier;
    }

    public abstract class R2ModelElement
    {
        internal string _path;
        internal EA.Element _element;

        public R2ModelElement(string path, EA.Element element)
        {
            _path = path;
            _element = element;
        }

        public string Path
        {
            get { return _path; }
        }

        public string Stereotype
        {
            get { return _element.Stereotype; }
        }

        public void Update()
        {
            _element.Update();
        }
    }

    public class R2Section : R2ModelElement
    {
        public R2Section(string path, EA.Element element) : base(path, element)
        {
            string notes = element.Notes;
            Dictionary<string, string> noteParts = R2Model.splitNotes(notes);
            _overview = noteParts.ContainsKey("OV") ? noteParts["OV"] : "";
            _example = noteParts.ContainsKey("EX") ? noteParts["EX"] : "";
            _actors = noteParts.ContainsKey("AC") ? noteParts["AC"] : "";
        }

        private void updateElementNotes()
        {
            string notes = string.Format("$OV${0}$EX${1}$AC${2}", _overview, _example, _actors);
            _element.Notes = notes;
        }

        private string _overview;
        private string _example;
        private string _actors;

        public string Name
        {
            get { return _element.Name; }
            set { _element.Name = value; }
        }

        public string SectionID
        {
            get { return _element.Alias; }
            set { _element.Alias = value; }
        }

        public string Overview
        {
            get { return _overview; }
            set { _overview = value; updateElementNotes(); }
        }

        public string Example
        {
            get { return _example; }
            set { _example = value; updateElementNotes(); }
        }

        public string Actors
        {
            get { return _actors; }
            set { _actors = value; updateElementNotes(); }
        }
    }

    public class R2Function : R2ModelElement
    {
        public R2Function(string path, EA.Element element) : base(path, element)
        {
            string notes = element.Notes;
            Dictionary<string, string> noteParts = R2Model.splitNotes(notes);
            _statement = noteParts.ContainsKey("ST") ? noteParts["ST"] : "";
            _description = noteParts.ContainsKey("DE") ? noteParts["DE"] : "";
            _example = noteParts.ContainsKey("EX") ? noteParts["EX"] : "";
        }

        private void updateElementNotes()
        {
            string notes = string.Format("$ST${0}$DE${1}$EX${2}", _statement, _description, _example);
            _element.Notes = notes;
        }

        private string _statement;
        private string _description;
        private string _example;

        public string Name
        {
            get { return _element.Name; }
            set { _element.Name = value; }
        }

        public string FunctionID
        {
            get { return _element.Alias; }
            set { _element.Alias = value; }
        }

        public string Statement
        {
            get { return _statement; }
            set { _statement = value; updateElementNotes(); }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; updateElementNotes(); }
        }

        public string Example
        {
            get { return _example; }
            set { _example = value; updateElementNotes(); }
        }
    }

    public class R2Criterion : R2ModelElement
    {
        public const string TV_ROW = "Row";
        public const string TV_DEPENDENT = "Dependent";
        public const string TV_CONDITIONAL = "Conditional";
        public const string TV_OPTIONALITY = "Optionality";

        public R2Criterion(string path, EA.Element element) : base(path, element)
        {
        }

        public virtual string Name
        {
            get { return _element.Name; }
        }

        public virtual string FunctionID
        {
            get
            {
                int sepIdx = _element.Name.IndexOf('#');
                return _element.Name.Substring(0, sepIdx);
            }
        }

        public virtual decimal CriterionID
        {
            get
            {
                int sepIdx = _element.Name.IndexOf('#');
                int sepIdx2 = _element.Name.IndexOf(' ', sepIdx);
                if (sepIdx2 == -1) sepIdx2 = _element.Name.Length;
                return decimal.Parse(_element.Name.Substring(sepIdx + 1, sepIdx2 - sepIdx - 1));
            }
            set
            {
                int sepIdx = _element.Name.IndexOf('#');
                string functionID = _element.Name.Substring(0, sepIdx);
                _element.Name = string.Format("{0}#{1:00}", functionID, value);
            }
        }

        public virtual string Text
        {
            get { return _element.Notes; }
            set { _element.Notes = value; }
        }

        public virtual decimal Row
        {
            get { return decimal.Parse(EAHelper.getTaggedValue(_element, TV_ROW, "0")); }
            set { EAHelper.updateTaggedValue(_element, TV_ROW, value.ToString()); }
        }

        public virtual bool Conditional
        {
            get { return "Y".Equals(EAHelper.getTaggedValue(_element, TV_CONDITIONAL, "N")); }
            set { EAHelper.updateTaggedValue(_element, TV_CONDITIONAL, value ? "Y" : "N"); }
        }

        public virtual bool Dependent
        {
            get { return "Y".Equals(EAHelper.getTaggedValue(_element, TV_DEPENDENT, "N")); }
            set { EAHelper.updateTaggedValue(_element, TV_DEPENDENT, value ? "Y" : "N"); }
        }            

        // Don't use Enum
        // We know Optionality can be extended in profiles
        public virtual string Optionality
        {
            get { return EAHelper.getTaggedValue(_element, TV_OPTIONALITY, ""); }
            set { EAHelper.updateTaggedValue(_element, TV_OPTIONALITY, value); }
        }
    }

    /**
     * Changes are recorded in the ciElement.
     */
    public class R2CriterionCI : R2Criterion, CompilerInstruction
    {
        private EA.Element _ciElement;

        public R2CriterionCI(string path, EA.Element ciElement, EA.Element element) : base(path, element)
        {
            _ciElement = ciElement;
        }

        public override string Name
        {
            get { return _ciElement.Name; }
        }

        public override string FunctionID
        {
            get
            {
                int sepIdx = _ciElement.Name.IndexOf('#');
                return _ciElement.Name.Substring(0, sepIdx);
            }
        }

        public override decimal CriterionID
        {
            get
            {
                int sepIdx = _ciElement.Name.IndexOf('#');
                int sepIdx2 = _ciElement.Name.IndexOf(' ', sepIdx);
                if (sepIdx2 == -1) sepIdx2 = _ciElement.Name.Length;
                return decimal.Parse(_ciElement.Name.Substring(sepIdx + 1, sepIdx2 - sepIdx - 1));
            }
            set
            {
                int sepIdx = _ciElement.Name.IndexOf('#');
                string functionID = _ciElement.Name.Substring(0, sepIdx);
                _ciElement.Name = string.Format("{0}#{1:00}", functionID, value);
            }
        }

        public override string Text
        {
            get
            {
                if (string.IsNullOrEmpty(_ciElement.Notes))
                {
                    return base.Text;
                }
                else
                {
                    return _ciElement.Notes;
                }
            }
            set
            {
                if (value.Equals(base.Text))
                {
                    _ciElement.Notes = "";
                }
                else
                {
                    _ciElement.Notes = value;
                }
                _ciElement.Update();
            }
        }

        public override decimal Row
        {
            get { return decimal.Parse(EAHelper.getTaggedValue(_ciElement, TV_ROW, base.Row.ToString())); }
            set
            {
                if (value.Equals(base.Row))
                {
                    EAHelper.removeTaggedValue(_ciElement, TV_ROW);
                }
                else
                {
                    EAHelper.updateTaggedValue(_ciElement, TV_ROW, value.ToString());
                }
            }
        }

        public override bool Conditional
        {
            get
            {
                string conditionalValue = EAHelper.getTaggedValue(_ciElement, TV_CONDITIONAL, null);
                if (conditionalValue == null)
                {
                    return base.Conditional;
                }
                else
                {
                    return "Y".Equals(conditionalValue);
                }
            }
            set
            {
                if (value.Equals(base.Conditional))
                {
                    EAHelper.removeTaggedValue(_ciElement, TV_CONDITIONAL);
                }
                else
                {
                    EAHelper.updateTaggedValue(_ciElement, TV_CONDITIONAL, value ? "Y" : "N");
                }
            }
        }

        public override bool Dependent
        {
            get
            {
                string dependentValue = EAHelper.getTaggedValue(_ciElement, TV_DEPENDENT, null);
                if (dependentValue == null)
                {
                    return base.Dependent;
                }
                else
                {
                    return "Y".Equals(dependentValue);
                }
            }
            set
            {
                if (value.Equals(base.Dependent))
                {
                    EAHelper.removeTaggedValue(_ciElement, TV_DEPENDENT);
                }
                else
                {
                    EAHelper.updateTaggedValue(_ciElement, TV_DEPENDENT, value ? "Y" : "N");
                }
            }
        }

        public override string Optionality
        {
            get
            {
                return EAHelper.getTaggedValue(_ciElement, TV_OPTIONALITY, base.Optionality);
            }
            set
            {
                if (value.Equals(base.Optionality))
                {
                    EAHelper.removeTaggedValue(_ciElement, TV_OPTIONALITY);
                }
                else
                {
                    EAHelper.updateTaggedValue(_ciElement, TV_OPTIONALITY, value);
                }
            }
        }
    }

    /**
     * Changes are recorded in the ciElement.
     */
    public class R2FunctionCI : R2Function, CompilerInstruction
    {
        private EA.Element _ciElement;

        public R2FunctionCI(string path, EA.Element ciElement, EA.Element element) : base(path, element)
        {
            _ciElement = ciElement;
        }
    }

    /**
     * Changes are recorded in the ciElement.
     */
    public class R2SectionCI : R2Section, CompilerInstruction
    {
        private EA.Element _ciElement;

        public R2SectionCI(string path, EA.Element ciElement, EA.Element element): base(path, element)
        {
            _ciElement = ciElement;
        }
    }
}
