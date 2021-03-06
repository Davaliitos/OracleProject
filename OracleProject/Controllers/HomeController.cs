﻿using OracleProject.Models;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OracleProject.Controllers
{
    public class HomeController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();
        double[,] globalDistances;
        double[] distanceToEnd;
        double[,] distanceToStart;


        //Makes a relation between the clusterId in the database and the locationId
        int[] clusterId = { 101, 102, 103, 104, 105 };
        public double[] distancesToEnd(int numberOfPoints)
        {
            double[] distances = new double[numberOfPoints];
            for (int i = 0; i < numberOfPoints; i++)
            {
                double tmp = db.distances.Where(x => x.ID_S == i && x.ID_E == 100).FirstOrDefault().duration;
                distances[i] = tmp;
            }
            return distances;
        }
        //Loads the array with distances to the start
        public double[,] distancesToStart(int numberOfPoints, int clusters)
        {
            double[,] distances = new double[numberOfPoints, clusters];
            for (int i = 0; i < numberOfPoints; i++)
            {
                for (int j = 0; j < clusters; j++)
                {
                    int id = clusterId[j];
                    distances[i, j] = db.distances.Where(x => x.ID_E == id && x.ID_S == i).FirstOrDefault().duration;
                }
            }

            return distances;

        }


        //Fills an array with the distances
        public double[,] loadDistances(int numberOfPoints)
        {
            double[,] distance = new double[numberOfPoints, numberOfPoints];
            var distances = db.distances.ToList();
            for (int i = 0; i < numberOfPoints; i++)
            {
                for (int j = 0; j < numberOfPoints; j++)
                {
                    if (i == 48 && j == 43)
                    {

                    }
                    var d = distances.Where(x => x.ID_S == i && x.ID_E == j).FirstOrDefault();
                    double time = 0;
                    int ID_E = i;
                    int ID_S = j;
                    if (d != null)
                    {
                        time = d.duration;
                    }

                    distance[ID_E, ID_S] = time;
                }

            }
            return distance;
        }


        //Fills array with negatives
        public int[] createArrayWithNegatives(int size)
        {
            int[] array = new int[size];
            for (int i = 0; i < size; i++)
            {
                array[i] = -1;
            }
            return array;
        }

        //Shuffles the permutation
        public void shuffle(int[] array, int size)
        {
            Random random = new Random();
            for (int i = 2; i <= size; i++)
            {
                int tmp = array[i - 1];
                int j = random.Next(2, size);
                array[i - 1] = array[j - 1];
                array[j - 1] = tmp;
            }

        }

        public bool AreEqualPermutations(int[] permutation1, int[] permutation2, int size)
        {
            if (permutation1 == null || permutation1 == null)
            {
                return false;
            }
            for (int i = 1; i < size - 1; i++)
            {
                if (permutation1[i] != permutation2[i])
                {
                    return false;
                }
            }
            return true;
        }
        //Checks if permutation exists in array
        public bool PermutationAlreadyExists(int[] permutation, int size, int totalChromosomes, Chromosome[] chromosomes)
        {
            for (int i = 0; i < totalChromosomes; i++)
            {
                if (chromosomes[i].permutation == null)
                {
                    continue;
                }
                if (AreEqualPermutations(permutation, chromosomes[i].permutation, size))
                {
                    return true;
                }
            }
            return false;
        }
        //Generates RandomPermutation
        public int[] GenerateRandomPermutation(int size, int seedParameter, int totalChromosomes, Chromosome[] chromosomes, Point[] points)
        {
            int[] permutation = new int[size];
            permutation[size - 1] = 100;
            permutation[0] = clusterId[points[0].cluster];
            for (int i = 0; i < size - 2; i++)
            {
                permutation[i + 1] = points[i].id;
            }

            do
            {
                shuffle(permutation, size - 1);
            }
            while (PermutationAlreadyExists(permutation, size, totalChromosomes, chromosomes));

            return permutation;
        }
        //Calculates time
        public void setChromosomeTotalTime(Chromosome chromosome, Point[] points, int numberOfPoints)
        {
            double totalTime = 0;
            int[] permutation = chromosome.permutation;
            //if(numberOfPoints-2 == 0)
            //{
            //    int point1 = permutation[1];
            //    totalTime += distanceToEnd[point1];
            //}
            totalTime += distanceToStart[permutation[1], points[0].cluster];

            for (int i = 1; i < numberOfPoints; i++)
            {
                int point1 = permutation[i];
                int point2 = permutation[i + 1];
                var time = globalDistances[point1, point2];
                if (i + 1 >= numberOfPoints)
                {
                    time += distanceToEnd[point2];
                }
                totalTime += time;
            }


            chromosome.totalDistance = totalTime;
        }



        //Creates a randomChromosome
        public void CreateRandomChromosome(Chromosome chromosome, int seedParameter, int numberPoints, Point[] points, int generation, int totalChromosomes, Chromosome[] chromosomes)
        {
            chromosome.permutation = GenerateRandomPermutation(numberPoints + 2, seedParameter, totalChromosomes, chromosomes, points);
            setChromosomeTotalTime(chromosome, points, numberPoints);


        }
        //Picks a random index
        public void pickRandomIndexRange(int ptrInd1, int ptrInd2, int size, int seedParameter)
        {
            Random random = new Random(seedParameter);
            ptrInd1 = random.Next(0, size / 2);
            ptrInd2 = ptrInd1 + random.Next(0, size / 2);
        }
        //Checks if the value is in range
        public bool isInRange(int value, int lowValue, int highValue)
        {
            return (value >= lowValue && value <= highValue);
        }
        //Passes the values of the first parent
        public void passSomeValuesDirectly(int size, int[] newArray, int leftIndex, int rightIndex, int[] parentArr)
        {
            for (int i = 1; i <= size; i++)
            {
                if (isInRange(i, leftIndex, rightIndex))
                {
                    newArray[i] = parentArr[i];
                }
                else
                {
                    newArray[i] = -1;
                }
            }
        }
        //Checks if the valueExistsInAnArray
        public bool valueExistsInArray(int size, int value, int[] array)
        {
            for (int i = 1; i < size; i++)
            {
                if (array[i] == value)
                {
                    return true;
                }
            }
            return false;
        }
        //Copies the values of the second parent
        public void passTheRestOfValues(int size, int[] newArray, int leftIndex, int rightIndex, int[] parentArr)
        {
            int parentArrIndex = 1;
            for (int i = 1; i <= size; i++)
            {
                if (isInRange(i, leftIndex, rightIndex))
                {
                    continue;
                }
                while (parentArrIndex < size && valueExistsInArray(size, parentArr[parentArrIndex], newArray))
                    parentArrIndex++;

                newArray[i] = parentArr[parentArrIndex];

            }

            newArray[0] = parentArr[0];
            newArray[size + 1] = 100;
        }

        //Finds the location in the array of a number
        public int FindInArray(int[] array, int value)
        {
            int index = -1;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == value)
                {
                    index = i;
                }

            }
            return index;
        }

        //Performs the ScxCrossover
        public int[] scxCrossOver(int size, int[] arr1, int[] arr2, int seedParameter)
        {
            //Creates an array with negatives
            int[] newArray = new int[size + 2];
            newArray = createArrayWithNegatives(size + 2);

            //The start and end are always the same
            newArray[0] = arr1[0];
            newArray[size + 1] = 100;

            //The node is always the first node of the first parent.
            int node = arr1[0];
            
            int index = 2;

            //Gets the cluster
            int cluster = FindInArray(clusterId, node);

            //gets the index next to where the node appears
            int index1 = FindInArray(arr1, node) + 1;
            int index2 = FindInArray(arr2, node) + 1;

            //If the node next to the array is the last, it starts from the beggining
            if (index2 == size + 1)
            {
                index2 = 0;
            }
            //gest the distance between the node and the one next
            double distance1 = distanceToStart[arr1[index1], cluster];
            double distance2 = distanceToStart[arr2[index2], cluster];

            //The one with less distance is the one that is stored
            if (distance1 > distance2)
            {
                newArray[index1] = arr2[index2];
                node = arr2[index2];

            }
            else
            {
                newArray[index1] = arr1[index1];
                node = arr1[index1];

            }
            for (int i = 1; i < size; i++)
            {
                index1 = FindInArray(arr1, node) + 1;
                index2 = FindInArray(arr2, node) + 1;
                if (index2 >= size + 1)
                {
                    index2 = 1;
                }
                if (index1 >= size + 1)
                {
                    index1 = 1;
                }
                while (FindInArray(newArray, arr1[index1]) != -1 && FindInArray(newArray, arr2[index2]) != -1)
                {
                    if (index1 != size)
                    {
                        index1++;
                    }
                    else
                    {
                        index1 = 1;
                    }
                    if (index2 != size)
                    {
                        index2++;
                    }
                    else
                    {
                        index2 = 1;
                    }
                }



                distance1 = globalDistances[node, arr1[index1]];
                distance2 = globalDistances[node, arr2[index2]];
                if (distance1 > distance2 && FindInArray(newArray, arr2[index2]) == -1)
                {
                    newArray[index] = arr2[index2];
                    node = arr2[index2];
                }
                else if (FindInArray(newArray, arr1[index1]) == -1)
                {
                    newArray[index] = arr1[index1];
                    node = arr1[index1];
                }
                else if (FindInArray(newArray, arr2[index2]) == -1)
                {
                    if (distance1 > distance2)
                    {
                        newArray[index] = arr1[index1];
                        node = arr1[index1];
                    }
                    else
                    {
                        newArray[index] = arr2[index2];
                        node = arr2[index2];
                    }
                }
                else
                {
                    newArray[index] = arr1[index1];
                    node = arr1[index1];
                }
                index++;
            }
            return newArray;
        }


        //Selects a random value, All the values from the first parent until the random value, are copied to the children. The mother fills the rest
        public int[] performDoubleCrossOver(int size, int[] arr1, int[] arr2, int seedParameter)
        {
            int[] newArray = new int[size + 2];
            int randomIndex;
            Random random = new Random(seedParameter);
            randomIndex = random.Next(1, size - 1);
            for (int i = 0; i < randomIndex; i++)
            {
                newArray[i] = arr1[i];
            }
            int parentArrIndex = 1;
            for (int i = randomIndex; i <= size; i++)
            {
                while (parentArrIndex < size && valueExistsInArray(size, arr2[parentArrIndex], newArray))
                    parentArrIndex++;

                newArray[i] = arr2[parentArrIndex];
            }
            newArray[size + 1] = 100;
            return newArray;
        }

        //Ordered CrossOver. 2 values are selected. THey represent the range that is going to be copyed from the first parent
        public int[] PerformOrderedCrossOver(int size, int[] arr1, int[] arr2, int seedParameter, Chromosome[] chromosomes, int totalChromosomes)
        {

            int[] newArray = new int[size + 2];
            int[] arr1SubIndex = new int[2];
            Random random = new Random(seedParameter);
            do
            {
                arr1SubIndex[0] = random.Next(1, (size - 1) / 2);
                arr1SubIndex[1] = arr1SubIndex[0] + random.Next(1, (size - 1) / 2);
            }
            while (arr1SubIndex[0] == arr1SubIndex[1] && arr1SubIndex[0] != 0);
            //pickRandomIndexRange(arr1SubIndex[0], arr1SubIndex[1], size, seedParameter);
            if (arr1SubIndex[0] == 1)
            {

            }



            passSomeValuesDirectly(size, newArray, arr1SubIndex[0], arr1SubIndex[1], arr1);
            passTheRestOfValues(size, newArray, arr1SubIndex[0], arr1SubIndex[1], arr2);


            return newArray;

        }



        //Performs mutation. Swaps two different values from an array
        public void PerformMutation(int size, int[] arr)
        {
            int index1 = 1;
            int index2 = 1;
            Random random = new Random();
            index1 = random.Next(1, size);
            index2 = random.Next(1, size);
            int tmp = arr[index1 - 1];
            arr[index1 - 1] = arr[index2 - 1];
            arr[index2 - 1] = tmp;
        }





        public int[] CreatePermutationFromParents(Chromosome c1, Chromosome c2, int childId, int totalChromosomes, Chromosome[] chromosomes, int type)
        {
            Random r = new Random(childId);
            int[] primes = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47 };
            int[] childPermutation;
            //The parameter type determines which of the 3 crossover methods is going to be used
            switch (type)
            {
                case 1:
                    {
                        childPermutation = PerformOrderedCrossOver(c1.pointsNumber, c1.permutation, c2.permutation, c1.generation * c2.generation + primes[r.Next() % 15], chromosomes, totalChromosomes);
                        break;
                    }
                case 2:
                    {
                        childPermutation = performDoubleCrossOver(c1.pointsNumber, c1.permutation, c2.permutation, c1.generation * c2.generation + primes[r.Next() % 15]);
                        break;
                    }
                case 3:
                    {
                        childPermutation = scxCrossOver(c1.pointsNumber, c1.permutation, c2.permutation, c1.generation * c2.generation + primes[r.Next() % 15]);
                        break;
                    }
                default:
                    {
                        childPermutation = PerformOrderedCrossOver(c1.pointsNumber, c1.permutation, c2.permutation, c1.generation * c2.generation + primes[r.Next() % 15], chromosomes, totalChromosomes);
                        break;
                    }
            }
            //Performs the mutation of the random chromosome
            if (childId % totalChromosomes == totalChromosomes / 2)
            {
                PerformMutation(c1.pointsNumber, childPermutation);

            }
            return childPermutation;
        }

        //Creates a ChildChromosome
        public Chromosome CreateChildChromosome(Chromosome c1, Chromosome c2, int totalChromosomes, int childId, int generation, int numberOfPoints, Point[] points, Chromosome[] chromosomes,int type)
        {
            Chromosome childChromosome = new Chromosome();
            //Creates the permution from the parents
            childChromosome.permutation = CreatePermutationFromParents(c1, c2, childId, totalChromosomes, chromosomes,type);
            childChromosome.pointsNumber = c1.pointsNumber;
            childChromosome.generation = generation;
            childChromosome.type = type;

            if (childChromosome.permutation[0] < 100)
            {
                return c1;
            }
            //Calculates time of the new chromosome
            setChromosomeTotalTime(childChromosome, points, numberOfPoints);
            return childChromosome;

        }
        //Calculates factorial
        public double factorial(int numberPoints)
        {
            double number = 1;
            for (int i = 1; i <= numberPoints; i++)
            {
                number = number * i;
            }
            return number;
        }

        //Gives the best Chromosome
        public Chromosome Solve(int numberPoints, Point[] points, int type)
        {
            double totalChromosomes;
            int totalGenerations;
            int bestChromosomes;



            //If the factorial of the number of Points is less than 100, it can be solved in one generation
            if (factorial(numberPoints) < 100)
            {
                totalChromosomes = factorial(numberPoints);
                Chromosome[] chromosomes2 = new Chromosome[(int)totalChromosomes];
                for (int i = 0; i < totalChromosomes; i++)
                {
                    Chromosome newChromosome = new Chromosome();
                    newChromosome.permutation = createArrayWithNegatives(numberPoints + 2);
                    newChromosome.generation = 0;
                    newChromosome.pointsNumber = numberPoints;
                    newChromosome.type = type;
                    chromosomes2[i] = newChromosome;
                }
                for (int i = 0; i < totalChromosomes; i++)
                {
                    CreateRandomChromosome(chromosomes2[i], i + 1, numberPoints, points, 0, (int)totalChromosomes, chromosomes2);
                }

                Array.Sort(chromosomes2, delegate (Chromosome chromosome1, Chromosome chromosome2)
                {
                    return chromosome1.totalDistance.CompareTo(chromosome2.totalDistance);
                });

                return chromosomes2[0];

            }
            //Number of chromomes
            totalChromosomes = 100;
            Chromosome[] chromosomes = new Chromosome[(int)totalChromosomes];

            //Generates each Chromosome with a permutation full of -1
            for (int i = 0; i < totalChromosomes; i++)
            {
                Chromosome newChromosome = new Chromosome();
                newChromosome.permutation = createArrayWithNegatives(numberPoints + 2);
                newChromosome.generation = 0;
                newChromosome.pointsNumber = numberPoints;
                newChromosome.type = type;
                chromosomes[i] = newChromosome;
            }

            //Creates a Random Chromosome
            for (int i = 0; i < totalChromosomes; i++)
            {
                CreateRandomChromosome(chromosomes[i], i + 1, numberPoints, points, 0, (int)totalChromosomes, chromosomes);
            }

            totalGenerations = 10000;
            bestChromosomes = (int)totalChromosomes / 4;

            //Chromosome test = new Chromosome();
            //int[] testP = { 101,10,38,14,25,5,15,22,13,44,24,11,1,0,100};
            //test.permutation = testP;
            //if(points[0].cluster == 0)
            //{
            //    chromosomes[99] = test;
            //    test.pointsNumber = 13;
            //    setChromosomeTotalTime(test, points, numberPoints);
            //}

            //Evaluate each generation
            for (int i = 0; i < totalGenerations; i++)
            {
                //Sorts the array of chromosomes by its duration
                Array.Sort(chromosomes, delegate (Chromosome chromosome1, Chromosome chromosome2)
                {
                    return chromosome1.totalDistance.CompareTo(chromosome2.totalDistance);
                });
                for (int k = 1; k <= 3; k++)
                {
                    for (int j = 0; j < 25; j++)
                    {
                        //Evaluates 75 children with 25 possible parents
                        int parent1Index = j;
                        int parent2Index = (j + k) % bestChromosomes;
                        int childChromosomeIndex = k * bestChromosomes + j;
                        chromosomes[childChromosomeIndex] = CreateChildChromosome(chromosomes[parent1Index], chromosomes[parent2Index], (int)totalChromosomes, childChromosomeIndex, i, numberPoints, points, chromosomes, type);
                    }
                }
            }
            //Sorts the array one final time
            Array.Sort(chromosomes, delegate (Chromosome chromosome1, Chromosome chromosome2)
            {
                return chromosome1.totalDistance.CompareTo(chromosome2.totalDistance);
            });
            //Returns the best chromosome
            return chromosomes[0];
        }

        //Load all points
        public Point[] LoadPoints(int numberPoints)
        {

            var locations = db.locations.ToList();
            Point[] points = new Point[numberPoints];
            for (int i = 0; i < numberPoints; i++)
            {
                Point newPoint = new Point();
                newPoint.id = locations[i].LocationId;
                newPoint.latitude = locations[i].lat;
                newPoint.longitude = locations[i].lng;
                newPoint.cluster = locations[i].cluster;
                points[i] = newPoint;
            }

            return points;
        }

        //Give points that belong to the cluster
        public Point[] getSubPoints(Point[] points, int cluster)
        {
            List<Point> subPoints = new List<Point>();
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].cluster == cluster)
                {
                    subPoints.Add(points[i]);
                }
            }
            Point[] p = new Point[subPoints.Count];
            for (int i = 0; i < subPoints.Count; i++)
            {
                p[i] = subPoints[i];
            }

            return p;
        }



        public ActionResult Index()
        {
            //Gets the number of clusters
            int clusters = db.locations.Select(x => x.cluster).Distinct().Count();

            //Defines the first number of points to be loaded
            int numberPoints = 50;

            //Defines the number of tries you want to test
            int timesToRun = 1;

            //Llena un arreglo de objetos  Point, con los valores de la base de datos
            Point[] points = new Point[numberPoints];
            points = LoadPoints(numberPoints);

            //Loads the distances between the points, between the start and each point, and between the end and each point.
            globalDistances = loadDistances(numberPoints);
            distanceToEnd = distancesToEnd(numberPoints);
            distanceToStart = distancesToStart(numberPoints, clusters);

            //Creates an array of chromosomes. 
            Chromosome[] chromosomes = new Chromosome[clusters];

            //Array to store the time each method took and the number that each method won the shortest duration
            double[] times = new double[3];
            double[] won = new double[3];

            //Array to store the locations in order. It is used in the View
            double[,,] coordinates2 = new double[clusters, 20, 2];
            //Stores the location that marks the end of the route
            int[] lastLocation = new int[clusters];

            //Loads the location info of the last point
            var end = db.locations.Where(x => x.LocationId == 100).FirstOrDefault();
            

            for(int k = 0; k< timesToRun; k++)
            {
                for (int i = 0; i < clusters; i++)
                {
                    
                    Chromosome shortestPathChromosome1 = new Chromosome();
                    Chromosome shortestPathChromosome2 = new Chromosome();
                    Chromosome shortestPathChromosome3 = new Chromosome();
                    Chromosome tmpChromosome = new Chromosome();
                    //Loads an array of subPoints, they have to be on the same cluster
                    Point[] subPoints = getSubPoints(points, i);
                    if (subPoints.Length != 0)
                    {
                        //For each cluster. Each of the 3 methods is going to be evaluated
                        var ordered = System.Diagnostics.Stopwatch.StartNew();
                        shortestPathChromosome1 = Solve(subPoints.Length, subPoints, 1);
                        ordered.Stop();
                        var doubleOrdered = System.Diagnostics.Stopwatch.StartNew();
                        shortestPathChromosome2 = Solve(subPoints.Length, subPoints, 2);
                        doubleOrdered.Stop();
                        var scx = System.Diagnostics.Stopwatch.StartNew();
                        shortestPathChromosome3 = Solve(subPoints.Length, subPoints, 3);
                        scx.Stop();
                        //Save the ammount of time each method took
                        times[0] += ordered.ElapsedMilliseconds;
                        times[1] += doubleOrdered.ElapsedMilliseconds;
                        times[2] += scx.ElapsedMilliseconds;



                        //Evaluate which method give the Chromosome with less duration, that Chromosome is stored in the Array.
                        if (shortestPathChromosome1.totalDistance < shortestPathChromosome2.totalDistance)
                        {
                            if (shortestPathChromosome1.totalDistance < shortestPathChromosome3.totalDistance)
                            {
                                tmpChromosome = shortestPathChromosome1;
                                won[0] += 1;

                            }
                            else
                            {
                                tmpChromosome = shortestPathChromosome3;
                                won[2] += 1;
                            }
                        }
                        else
                        {
                            if (shortestPathChromosome2.totalDistance < shortestPathChromosome3.totalDistance)
                            {
                                tmpChromosome = shortestPathChromosome2;
                                won[1] += 1;
                            }
                            else
                            {
                                tmpChromosome = shortestPathChromosome3;
                                won[2] += 1;
                            }
                        }

                        if(chromosomes[i] ==null)
                        {
                            chromosomes[i] = tmpChromosome;
                        }
                        else if(tmpChromosome.totalDistance < chromosomes[i].totalDistance)
                        {
                            chromosomes[i] = tmpChromosome;
                        }


                    }
                    else
                    {
                        continue;
                    }


                    
                }
            }


            //This method stores the winning chromosomes in the array that will be sent to the view
            for (int i = 0; i < clusters; i++)
            {
                int id = clusterId[i];
                var start = db.locations.Where(x => x.LocationId == id).FirstOrDefault();
                int[] locations = new int[chromosomes[i].permutation.Length];
                locations = chromosomes[i].permutation;
                for (int j = 1; j <= chromosomes[i].pointsNumber; j++)
                {
                    coordinates2[i, j, 0] = points[locations[j]].latitude;
                    coordinates2[i, j, 1] = points[locations[j]].longitude;
                }
                coordinates2[i, 0, 0] = start.lat;
                coordinates2[i, 0, 1] = start.lng;
                coordinates2[i, chromosomes[i].pointsNumber + 1, 0] = end.lat;
                coordinates2[i, chromosomes[i].pointsNumber + 1, 1] = end.lng;
                lastLocation[i] = chromosomes[i].pointsNumber + 2;
            }

            //saves the permutation in array to be sent to the view
            string message = "";
            List<List<int>> permutations = new List<List<int>>();
            double[] distance = new double[clusters];
            int[] method = new int[clusters];            
            for (int i = 0; i < chromosomes.Length; i++)
            {
                List<int> permutation = new List<int>();
                permutation = chromosomes[i].permutation.ToList();
                permutations.Add(permutation);
                distance[i] = chromosomes[i].totalDistance;
                method[i] = chromosomes[i].type;
            }

            //Calculate statistics
            won[0] = (double)decimal.Divide((decimal)won[0], clusters * timesToRun)*100;
            won[1] = (double)decimal.Divide((decimal)won[1], clusters * timesToRun) * 100;
            won[2] = (double)decimal.Divide((decimal)won[2], clusters * timesToRun) * 100;

            won[0] = Math.Round(won[0], 2);
            won[1] = Math.Round(won[1], 2);
            won[2] = Math.Round(won[2], 2);

            times[0] = (times[0] / (clusters * timesToRun));
            times[1] = (times[1] / (clusters * timesToRun));
            times[2] = (times[2] / (clusters * timesToRun));


            //Info to be sent to the View
            ViewBag.times = times;
            ViewBag.won = won;
            ViewBag.distance = distance;
            ViewBag.type = method;

            ViewBag.permutations = permutations;

            ViewBag.Count = clusters;
            int[] counter = new int[clusters];
            for (int i = 0; i < clusters; i++)
            {
                counter[i] = i;
            }
            ViewBag.Counter = counter;
            ViewBag.coordinates = coordinates2;
            ViewBag.lastLocation = lastLocation;
            string[] colors = { "purple", "aqua", "red", "green", "yellow", "olive", "blue", "silver", "teal" };
            ViewBag.colors = colors;
            ViewBag.message = message;
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }





    }
}