import React, { useEffect, useState } from "react";
import {
  Table,
  Card,
  message,
  Spin,
  Button,
  Modal,
  Input,
  InputNumber,
} from "antd";
import { taxGroupService } from "../services/TaxGroupService";

const TaxGroupDashboard = () => {
  const [taxGroups, setTaxGroups] = useState([]);
  const [loading, setLoading] = useState(true);

  // For viewing/editing
  const [isModalVisible, setIsModalVisible] = useState(false);
  const [selectedGroup, setSelectedGroup] = useState(null);
  const [isEditing, setIsEditing] = useState(false);
  const [editedGroup, setEditedGroup] = useState(null);

  // For adding
  const [isAddModalVisible, setIsAddModalVisible] = useState(false);
  const [newGroup, setNewGroup] = useState({ name: "", vat: null, ecoTax: null });

  // For delete confirmation
  const [isDeleteModalVisible, setIsDeleteModalVisible] = useState(false);
  const [deleteError, setDeleteError] = useState(null);

  const [errorMessage, setErrorMessage] = useState("");

  // Fetch all tax groups
  const fetchTaxGroups = async () => {
    try {
      const data = await taxGroupService.getAll();
      setTaxGroups(data);
    } catch (err) {
      console.error(err);
      message.error("Failed to load tax groups.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchTaxGroups();
  }, []);

  // Open view/edit modal
  const openModal = (group) => {
    setSelectedGroup(group);
    setEditedGroup({ ...group });
    setIsModalVisible(true);
    setIsEditing(false);
  };

  const closeModal = () => {
    setSelectedGroup(null);
    setIsModalVisible(false);
    setIsEditing(false);
  };

  const handleEdit = () => setIsEditing(true);

  // --- DELETE LOGIC ---
  const openDeleteModal = () => setIsDeleteModalVisible(true);
  const closeDeleteModal = () => setIsDeleteModalVisible(false);

  const handleDelete = async () => {
    if (!selectedGroup) return;
    try {
      await taxGroupService.delete(selectedGroup.id);
      message.success("Tax group deleted successfully!");
      closeDeleteModal();
      closeModal();
      fetchTaxGroups();
    } catch (err) {
      console.error(err);
      setDeleteError("Cannot delete tax group with one or more users in it!")
      message.error("Failed to delete tax group.");
    }
  };

  const handleSave = async () => {
    if (!editedGroup) return;
    setErrorMessage("");

    if (!editedGroup.name.trim()) {
      setErrorMessage("Name cannot be empty.");
      return;
    }
    if(editedGroup.vat==null){
      setErrorMessage("Please enter the vat tax %!")
      return;
    }
    if(editedGroup.vat<=0){
      setErrorMessage("Vat tax % cannot be 0 or negative")
      return;
    }
    if(editedGroup.ecoTax==null){
      setErrorMessage("Please enter the eco tax %!")
      return;
    }
    if(editedGroup.ecoTax<=0){
      setErrorMessage("Eco tax % cannot be 0 or negative")
      return;
    }
    try {
      await taxGroupService.update(editedGroup.id, editedGroup);
      message.success("Tax group updated successfully!");
      closeModal();
      fetchTaxGroups();
    } catch (err) {
      console.error(err);
      message.error("Failed to update tax group.");
    }
  };

  // Add logic
  const openAddModal = () => {
    setNewGroup({ name: "", vat: null, ecoTax: null });
    setErrorMessage("");
    setIsAddModalVisible(true);
  };

  const closeAddModal = () => setIsAddModalVisible(false);

  const handleAddGroup = async () => {
    if (!newGroup.name.trim()) {
      setErrorMessage("Name cannot be empty.");
      return;
    }
     if(newGroup.vat==null){
      setErrorMessage("Please enter the vat tax %!")
      return;
    }
    if(newGroup.vat<=0){
      setErrorMessage("Vat tax % cannot be 0 or negative")
      return;
    }
    if(newGroup.ecoTax==null){
      setErrorMessage("Please enter the eco tax %!")
      return;
    }
    if(newGroup.ecoTax<=0){
      setErrorMessage("Eco tax % cannot be 0 or negative")
      return;
    }
    try {
      await taxGroupService.create(newGroup);
      message.success("Tax group created successfully!");
      closeAddModal();
      fetchTaxGroups();
    } catch (err) {
      console.error(err);
      message.error("Failed to create tax group.");
    }
  };

  const columns = [
    { title: "ID", dataIndex: "id", key: "id", width: 80 },
    { title: "Name", dataIndex: "name", key: "name" },
    {
      title: "VAT (%)",
      dataIndex: "vat",
      key: "vat",
      render: (value) => (value != null ? `${value}%` : "—"),
    },
    {
      title: "Eco Tax (%)",
      dataIndex: "ecoTax",
      key: "ecoTax",
      render: (value) => (value != null ? `${value}%` : "—"),
    },
  ];

  return (
    <Card title="Tax Group Dashboard" className="w-96 shadow-md text-center">
      {loading ? (
        <div style={{ textAlign: "center", padding: 50 }}>
          <Spin size="large" />
        </div>
      ) : (
        <div style={{ background: "#fff", padding: 24, borderRadius: 8 }}>
          <Table
            dataSource={taxGroups}
            columns={columns}
            rowKey="id"
            pagination={{ pageSize: 8 }}
            onRow={(record) => ({
              onClick: () => openModal(record),
              style: { cursor: "pointer" },
            })}
          />
          <Button
            type="primary"
            style={{ marginTop: 16 }}
            onClick={openAddModal}
          >
            + Add New Tax Group
          </Button>
        </div>
      )}

      {/* VIEW / EDIT MODAL */}
      <Modal
        title={isEditing ? "Edit Tax Group" : selectedGroup?.name || "Tax Group Details"}
        open={isModalVisible}
        onCancel={closeModal}
        width={600}
        footer={
          isEditing
            ? [
                <Button key="cancel" onClick={() => setIsEditing(false)}>
                  Cancel
                </Button>,
                <Button key="save" type="primary" onClick={handleSave}>
                  Save Changes
                </Button>,
              ]
            : [
                <Button key="edit" type="primary" onClick={handleEdit}>
                  Edit
                </Button>,
                <Button danger onClick={openDeleteModal}>
                  Delete
                </Button>,
                <Button key="close" onClick={closeModal}>
                  Close
                </Button>,
              ]
        }
      >
        {editedGroup && (
          <>
            <p>
              <b>Name:</b>{" "}
              <Input
                value={editedGroup.name}
                onChange={(e) =>
                  setEditedGroup({ ...editedGroup, name: e.target.value })
                }
                disabled={!isEditing}
              />
            </p>
            <p>
              <b>VAT (%):</b>{" "}
              <InputNumber
                min={0}
                max={100}
                value={editedGroup.vat}
                onChange={(val) =>
                  setEditedGroup({ ...editedGroup, vat: val })
                }
                disabled={!isEditing}
              />
            </p>
            <p>
              <b>Eco Tax (%):</b>{" "}
              <InputNumber
                min={0}
                step={0.01}
                value={editedGroup.ecoTax}
                onChange={(val) =>
                  setEditedGroup({ ...editedGroup, ecoTax: val })
                }
                disabled={!isEditing}
              />
            </p>
            {errorMessage && <p style={{ color: "red" }}>{errorMessage}</p>}
          </>
        )}
      </Modal>

      {/* DELETE MODAL */}
      <Modal
        title="Delete Tax Group"
        open={isDeleteModalVisible}
        onCancel={closeDeleteModal}
        okText="Delete"
        okType="danger"
        onOk={handleDelete}
        cancelText="Cancel"
      >
        <p>
          Are you sure you want to delete the tax group "
          {selectedGroup?.name}"? This action cannot be undone.
          {deleteError && (
          <p style={{ color: "red", marginTop: 8 }}>{deleteError}</p>
        )}
        </p>
      </Modal>

      {/* ADD TAX GROUP MODAL */}
      <Modal
        title="Add New Tax Group"
        open={isAddModalVisible}
        onCancel={closeAddModal}
        width={600}
        footer={[
          <Button key="cancel" onClick={closeAddModal}>
            Cancel
          </Button>,
          <Button key="create" type="primary" onClick={handleAddGroup}>
            Create Tax Group
          </Button>,
        ]}
      >
        <p>
          <b>Name:</b>{" "}
          <Input
            value={newGroup.name}
            onChange={(e) =>
              setNewGroup({ ...newGroup, name: e.target.value })
            }
          />
        </p>
        <p>
          <b>VAT (%):</b>{" "}
          <InputNumber
            min={0}
            max={100}
            value={newGroup.vat}
            onChange={(val) => setNewGroup({ ...newGroup, vat: val })}
          />
        </p>
        <p>
          <b>Eco Tax (%):</b>{" "}
          <InputNumber
            min={0}
            step={0.01}
            value={newGroup.ecoTax}
            onChange={(val) => setNewGroup({ ...newGroup, ecoTax: val })}
          />
        </p>
        {errorMessage && (
          <p style={{ color: "red", marginTop: 8 }}>{errorMessage}</p>
        )}
      </Modal>
    </Card>
  );
};

export default TaxGroupDashboard;
