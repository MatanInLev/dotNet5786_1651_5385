namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Runtime.CompilerServices;

internal class CourierImplementation : ICourier
{
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Create(Courier item)
    {
        // Load the current list from the XML file
        List<Courier> couriers = XMLTools.LoadListFromXMLSerializer<Courier>(Config.courier_file_name);

        // Check for duplicates
        if (couriers.Any(c => c.Id == item.Id))
            throw new DalAlreadyExistsException($"Courier with ID {item.Id} already exists.");

        // Add the new item to the list
        couriers.Add(item);

        // Save the updated list back to the XML file
        XMLTools.SaveListToXMLSerializer(couriers, Config.courier_file_name);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public Courier? Read(int id)
    {
        // Load the list from XML and find the item
        List<Courier> couriers = XMLTools.LoadListFromXMLSerializer<Courier>(Config.courier_file_name);
        return couriers.FirstOrDefault(c => c.Id == id);
    }

    // Fix for CS0535: Implement Read(Func<Courier, bool> filter)
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Courier? Read(Func<Courier, bool> filter)
    {
        List<Courier> couriers = XMLTools.LoadListFromXMLSerializer<Courier>(Config.courier_file_name);
        return couriers.FirstOrDefault(filter);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public List<Courier> ReadAll()
    {
        // Return the entire list from the XML file
        return XMLTools.LoadListFromXMLSerializer<Courier>(Config.courier_file_name);
    }

    // Fix for CS0535: Implement ReadAll(Func<Courier, bool>? filter)
    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Courier> ReadAll(Func<Courier, bool>? filter = null)
    {
        List<Courier> couriers = XMLTools.LoadListFromXMLSerializer<Courier>(Config.courier_file_name);
        return filter == null ? couriers : couriers.Where(filter);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Update(Courier item)
    {
        // Load the current list from XML
        List<Courier> couriers = XMLTools.LoadListFromXMLSerializer<Courier>(Config.courier_file_name);

        // Find the index of the item to update
        int idx = couriers.FindIndex(c => c.Id == item.Id);
        if (idx == -1)
            throw new DalDoesNotExistException($"Courier with ID {item.Id} does not exist.");

        // Replace the old item with the new item
        couriers[idx] = item;

        // Save the updated list back to XML
        XMLTools.SaveListToXMLSerializer(couriers, Config.courier_file_name);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Delete(int id)
    {
        // Load the current list from XML
        List<Courier> couriers = XMLTools.LoadListFromXMLSerializer<Courier>(Config.courier_file_name);

        // Find the index of the item to delete
        int idx = couriers.FindIndex(c => c.Id == id);
        if (idx == -1)
            throw new DalDoesNotExistException($"Courier with ID {id} does not exist.");

        // Remove the item from the list
        couriers.RemoveAt(idx);

        // Save the updated list back to XML
        XMLTools.SaveListToXMLSerializer(couriers, Config.courier_file_name);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void DeleteAll()
    {
        // Create an empty list
        List<Courier> emptyList = new List<Courier>();

        // Save the empty list to the XML file, effectively clearing it
        XMLTools.SaveListToXMLSerializer(emptyList, Config.courier_file_name);
    }
}