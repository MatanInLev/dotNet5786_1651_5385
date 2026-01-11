namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

internal class DeliveryImplementation : IDelivery
{
    /// <summary>
    /// Create a new delivery record. Generates and assigns a new Id,
    /// then saves the updated list to the XML file.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Create(Delivery item)
    {
        // Load the current list from the XML file
        List<Delivery> deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.delivery_file_name);

        // Get the next ID from the config
        int newId = Config.NextDeliveryId;
        var deliveryToAdd = item with { Id = newId };

        // Add the new item to the list
        deliveries.Add(deliveryToAdd);

        // Save the updated list back to the XML file
        XMLTools.SaveListToXMLSerializer(deliveries, Config.delivery_file_name);
    }

    /// <summary>
    /// Delete delivery by id from the XML file. 
    /// Throws DalDoesNotExistException if missing.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Delete(int id)
    {
        // Load the current list from XML
        List<Delivery> deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.delivery_file_name);

        // Find the index of the item to delete
        int index = deliveries.FindIndex(d => d.Id == id);
        if (index == -1)
            throw new DalDoesNotExistException($"An object of type Delivery with ID: {id} does not exist.");

        // Remove the item
        deliveries.RemoveAt(index);

        // Save the updated list back to XML
        XMLTools.SaveListToXMLSerializer(deliveries, Config.delivery_file_name);
    }

    /// <summary>
    /// Remove all deliveries by saving an empty list to the XML file.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void DeleteAll()
    {
        // Create an empty list
        List<Delivery> emptyList = new List<Delivery>();

        // Save the empty list, overwriting the file
        XMLTools.SaveListToXMLSerializer(emptyList, Config.delivery_file_name);
    }

    /// <summary>
    /// Read a single delivery by id from the XML file. Returns null when not found.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Delivery? Read(int id)
    {
        // Load the list from XML and find the item
        List<Delivery> deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.delivery_file_name);
        return deliveries.FirstOrDefault(d => d.Id == id);
    }


    /// <summary>
    /// Read a single delivery matching a filter from the XML file. Returns null when not found.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Delivery? Read(Func<Delivery, bool> filter)
    {
        List<Delivery> deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.delivery_file_name);
        return deliveries.FirstOrDefault(filter);
    }

    /// <summary>
    /// Read all deliveries matching a filter from the XML file.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Delivery> ReadAll(Func<Delivery, bool>? filter = null)
    {
        List<Delivery> deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.delivery_file_name);
        return filter == null ? deliveries : deliveries.Where(filter);
    }

    /// <summary>
    /// Replace an existing delivery record in the XML file.
    /// Throws DalDoesNotExistException when the id is not present.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Update(Delivery item)
    {
        // Load the current list from XML
        List<Delivery> deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.delivery_file_name);

        // Find the index of the item to update
        int index = deliveries.FindIndex(d => d.Id == item.Id);
        if (index == -1)
            throw new DalDoesNotExistException($"An object of type Delivery with ID: {item.Id} does not exist.");

        // Replace the old item with the new item
        deliveries[index] = item;

        // Save the updated list back to XML
        XMLTools.SaveListToXMLSerializer(deliveries, Config.delivery_file_name);
    }
}
