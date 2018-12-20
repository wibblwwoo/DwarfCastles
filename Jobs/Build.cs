using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace DwarfCastles.Jobs
{
    public class Build : Job
    {
        private string BuildingName { get; }
        private IList<Tag> ResourcesRequired;

        private IList<Entity> ResourcesCaptured;

        private readonly Point BuildSite;

        private double WorkRequired;

        public Build(Point location, string buildingName)
        {
            ResourcesCaptured = new List<Entity>();
            BuildingName = buildingName;
            BuildSite = location;
            var buildable = ResourceMasterList.GetDefault(buildingName).GetTag("buildable");
            ResourcesRequired = buildable.GetTag("resources").SubTags;
            WorkRequired = buildable.GetTag("workrequired").Value.GetDouble();
        }

        private Tag NextRequiredResource()
        {
            foreach (var r in ResourcesRequired)
            {
                var capturedAmount = CountMatchingResources(r);
                if (r.GetTag("amount").Value.GetDouble() > capturedAmount)
                {
                    return r;
                }
            }

            return null;
        }

        private int CountMatchingResources(Tag t)
        {
            return ResourcesCaptured.Count(r => Matches(t, r));
        }

        public override void TakeOwnership(Actor a)
        {
            Owner = a;
            Owner.Map.Impassables[BuildSite.X, BuildSite.Y] = true; // When someone starts working on it, make the location impassable
            GenerateNextStep();
            if (Location != BuildSite && SubJobs.Count == 0)
            {
                Logger.Log("Interrupting Owner in Build Job");
                Owner.Inturupt();
            }
        }

        private void GenerateNextStep()
        {
            var next = NextRequiredResource();
            if (next == null)
            {
                Location = BuildSite;
            }
            else
            {
                CollectResource(next);
            }
        }

        public override Point GetLocation()
        {
            if (SubJobs.Count == 0)
            {
                Logger.Log("Build job returning it's own location");
                return Location;
            }
            Logger.Log("Build job returning Subjob Location");
            return SubJobs.Peek().GetLocation();
        }

        public override void Work()
        {
            if (SubJobs.Count == 0)
            {
                Logger.Log("Build doing self work");
                WorkRequired--;
                if (WorkRequired <= 0)
                {
                    Logger.Log("Build Finishing");
                    Finish();
                }
            }
            else
            {
                Logger.Log("Build doing subwork");
                SubJobs.Peek().Work();
                Location = SubJobs.Peek().GetLocation();
                if (SubJobs.Peek().Completed)
                {
                    if (SubJobs.Peek() is Haul)
                    {
                        var j = (Haul) SubJobs.Peek();
                        foreach (var c in j.Carried)
                        {
                            ResourcesCaptured.Add(c);
                        }

                        j.Carried = new List<Entity>();
                        j.ReleaseOwnership();
                    }

                    SubJobs.Dequeue();
                    GenerateNextStep();
                }
            }
        }

        private void CollectResource(Tag resourceTag)
        {
            var amount = resourceTag.GetTag("amount").Value.GetDouble();

            IList<int> entityIds = new List<int>();
            var found = 0;
            foreach (var e in Owner.Map.Entities)
            {
                if (e.Locked)
                {
                    continue;
                }

                var carriable = e.GetTag("attributes.carriable");
                if (carriable != null)
                {
                    if (!carriable.Value.GetBool())
                    {
                        continue;
                    }
                }

                if (Matches(resourceTag, e))
                {
                    entityIds.Add(e.Id);
                    found++;
                }

                if (found == amount)
                {
                    Logger.Log("Found the correct amount of resources needed to build");
                    break;
                }
            }

            if (found == amount)
            {
                SubJobs.Enqueue(new Haul(BuildSite, entityIds, Owner));
                SubJobs.Peek().TakeOwnership(Owner);
            }
        }

        private bool Matches(Tag ResourceTag, Entity e)
        {
            if (ResourceTag.GetTag("type") != null && e.HasType(ResourceTag.GetTag("type").Value.GetString()))
            {
                return true;
            }

            if (ResourceTag.GetTag("name") != null && ResourceTag.GetTag("name").Value.GetString() == e.Name)
            {
                return true;
            }

            return false;
        }

        protected override void Finish()
        {
            Owner.Map.AddEntity(ResourceMasterList.GetDefaultClone(BuildingName), BuildSite);
            Completed = true;
        }
    }
}