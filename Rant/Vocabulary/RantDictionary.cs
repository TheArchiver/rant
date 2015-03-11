﻿using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rant.Vocabulary
{
    /// <summary>
    /// Represents a Rant dictionary. This is the default dictionary type used by Rant.
    /// </summary>
    public sealed class RantDictionary : IRantDictionary
    {
        private readonly Dictionary<string, RantDictionaryTable> _tables;

        /// <summary>
        /// Creates a new RantDictionary object from the specified dictionary collection.
        /// </summary>
        /// <param name="dics"></param>
        public RantDictionary(IEnumerable<RantDictionaryTable> dics)
        {
            _tables = new Dictionary<string, RantDictionaryTable>();

            if (dics == null) return;

            RantDictionaryTable table;
            foreach (var list in dics)
            {
                if (_tables.TryGetValue(list.Name, out table))
                {
                    table.Merge(list);
                }
                else
                {
                    _tables[list.Name] = list;
                }
            }
        }

        /// <summary>
        /// Adds a new RantDictionaryTable object to the collection.
        /// </summary>
        /// <param name="table"></param>
        public void AddTable(RantDictionaryTable table)
        {
            RantDictionaryTable oldTable;
            if (_tables.TryGetValue(table.Name, out oldTable))
            {
                oldTable.Merge(table);
            }
            else
            {
                _tables[table.Name] = table;
            }
        }

        /// <summary>
        /// Enumerates the tables contained in the current RantDictionary instance.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RantDictionaryTable> GetTables()
        {
            foreach (var pair in _tables) yield return pair.Value;
        } 

        /// <summary>
        /// Loads all dictionary (.dic) files from the specified directory and returns a Dictionary object that contains the loaded data.
        /// </summary>
        /// <param name="directory">The directory from which to load dictionaries.</param>
        /// <param name="filter">Indicates whether dictionary entries marked with the #nsfw flag should be loaded.</param>
        /// <returns></returns>
        public static RantDictionary FromDirectory(string directory, NsfwFilter filter)
        {
            return new RantDictionary(Directory.GetFiles(directory, "*.dic", SearchOption.AllDirectories).Select(file => RantDictionaryTable.FromFile(file, filter)).ToList());
        }

        /// <summary>
        /// Loads all dictionary (.dic) files from the specified directories and returns a Dictionary object that contains the loaded data.
        /// </summary>
        /// <param name="directories">The directories from which to load dictionaries.</param>
        /// <returns></returns>
        public static RantDictionary FromMultiDirectory(params string[] directories)
        {
            return new RantDictionary(directories.SelectMany(path => Directory.GetFiles(path, "*.dic", SearchOption.AllDirectories)).Select(file => RantDictionaryTable.FromFile(file)));
        }

        /// <summary>
        /// Loads all dictionary (.dic) files from the specified directories and returns a Dictionary object that contains the loaded data.
        /// </summary>
        /// <param name="directories">The directories from which to load dictionaries.</param>
        /// <param name="filter">Indicates whether dictionary entries marked with the #nsfw flag should be loaded.</param>
        /// <returns></returns>
        public static RantDictionary FromMultiDirectory(string[] directories, NsfwFilter filter)
        {
            return new RantDictionary(directories.SelectMany(path => Directory.GetFiles(path, "*.dic", SearchOption.AllDirectories)).Select(file => RantDictionaryTable.FromFile(file, filter)));
        }

        /// <summary>
        /// Queries the Dictionary according to the specified criteria and returns a random match.
        /// </summary>
        /// <param name="rng">The random number generator to randomize the match with.</param>
        /// <param name="query">The search criteria to use.</param>
        /// <param name="syncState">The state object to use for carrier synchronization.</param>
        /// <returns></returns>
        public string Query(RNG rng, Query query, QueryState syncState)
        {
            RantDictionaryTable table;
            return !_tables.TryGetValue(query.Name, out table) 
                ? "[Missing Table]" 
                : table.Query(rng, query, syncState);
        }
    }
}