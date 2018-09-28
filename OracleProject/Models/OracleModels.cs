using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OracleProject.Models
{
    public class Point
    {
        public int id { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public int cluster { get; set; }
    }

    public class Chromosome
    {
        public int pointsNumber { get; set; }
        public int[] permutation { get; set; }
        public double totalDistance { get; set; }
        public int generation { get; set; }
        public int type { get; set; }
    }

    public class Distances
    {
        [Key]
        public int Id { get; set; }
        public int ID_S { get; set; }
        public int ID_E { get; set; }
        public double distance { get; set; }
        public double duration { get; set; }
    }

    public class Locations
    {
        [Key]
        public int Id { get; set; }
        public int LocationId { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }
        public int cluster { get; set; }
    }

}