using System.Collections.Generic;

namespace DwarfCastles
{
    /// <summary>
    /// Authors: Alec Mills and Josh DeMoss
    /// This class is an organizational tool for all objects to be able to
    /// store and retrieve diverse data without changing the class structure.
    /// </summary>
    public class Tag
    {
        public string Name { get; private set; }

        // These are possible values of tags. Only one will be used at a time
        // This fits the JSON format of value, array, or new object
        // TODO possibly make this only use what's needed
        public Value Value { get; set; }
        public IList<Tag> SubTags { get; }
        public IList<Value> ArrayValues { get; }

        #region Constructors

        public Tag()
        {
            Value = new Value();
            SubTags = new List<Tag>();
            ArrayValues = new List<Value>();
        }

        public Tag(string name) : this()
        {
            Name = name.ToLower();
        }

        #endregion Constructors

        #region Convienience Methods

        public void AddArrayValue(Value v)
        {
            ArrayValues.Add(v);
        }

        public Tag GetTag(string TagChain)
        {
            var Tags = TagChain.ToLower().Split('.');
            //Logger.Log($"Entering GetTag() with a Tag chain of length {Tags.Length}");
            var CurrentTag = this;
            foreach (var s in Tags)
            {
                //Logger.Log($"Searching for tag {s} in {CurrentTag.Name}");
                var Found = false;
                foreach (var tag in CurrentTag.SubTags)
                {
                    //Logger.Log($"Checking tag {tag.Name}");
                    if (tag.Name != s)
                        continue;
                    CurrentTag = tag;
                    Found = true;
                    break;
                }

                if (!Found)
                {
                    return null;
                }
            }

            return CurrentTag;
        }


        public void AddTag(Tag tag)
        {
            if (GetTag(tag.Name) != null && tag.Name != "") // Tags with "" are actually list items
            {
                var index = 0;
                for (var i = 0; i < SubTags.Count; i++)
                {
                    if (SubTags[i].Name == tag.Name)
                    {
                        index = i;
                        break;
                    }
                }

                Logger.Log($"Adding Tag of a similar name {tag.Name}");
                if (tag.SubTags.Count == 0)
                {
                    Logger.Log("Replacing Tag, as it is a value tag");
                    SubTags.RemoveAt(index);
                }
                else
                {
                    Logger.Log("Because there are subtags, merging tags");
                    foreach (var t in tag.SubTags)
                    {
                        SubTags[index].AddTag(t);
                    }

                    return;
                }
            }

            SubTags.Add(tag);
        }

        #endregion Convienience Methods

        public Tag Clone()
        {
            var clone = new Tag {Name = Name, Value = Value.Clone()};
            foreach (var subTag in SubTags)
            {
                clone.AddTag(subTag.Clone());
            }

            foreach (var v in ArrayValues)
            {
                clone.AddArrayValue(v.Clone());
            }

            return clone;
        }

        public override string ToString()
        {
            var builtString = "Tag Name: " + Name + "\n";
            var valOut = false;
            foreach (var t in SubTags)
            {
                builtString += t + "\n";
                valOut = true;
            }

            foreach (var v in ArrayValues)
            {
                builtString += v + "\n";
                valOut = true;
            }

            if (!valOut)
            {
                builtString += Value + "\n";
            }

            return builtString;
        }
    }
}