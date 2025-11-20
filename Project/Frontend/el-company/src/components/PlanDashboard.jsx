import React, { useEffect, useState } from "react";
import {
  Table,
  Card,
  message,
  Spin,
  Tag,
  Button,
  Modal,
  Input,
  InputNumber,
  Space,
} from "antd";
import { planService } from "../services/PlanService";

const PlansDashboard = () => {
  const [plans, setPlans] = useState([]);
  const [loading, setLoading] = useState(true);

  // For viewing/editing an existing plan
  const [isModalVisible, setIsModalVisible] = useState(false);
  const [selectedPlan, setSelectedPlan] = useState(null);
  const [isEditing, setIsEditing] = useState(false);
  const [editedPlan, setEditedPlan] = useState(null);
  
  // For adding a new plan
  const [isAddModalVisible, setIsAddModalVisible] = useState(false);
  const [newPlan, setNewPlan] = useState({ name: "", discount: null, prices: [] });

  const [errorMessage, setErrorMessage] = useState("");

  // For delete confirmation
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [deleteError, setDeleteError] = useState(null);

  const fetchPlans = async () => {
    try {
      const response = await planService.getAll();
      setPlans(response);
    } catch (error) {
      console.error(error);
      message.error("Failed to load plans.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchPlans();
  }, []);

  // --- VIEW / EDIT LOGIC ---
  const openModal = (plan) => {
    setSelectedPlan(plan);
    setEditedPlan({ ...plan, prices: plan.prices.map((p) => ({ ...p })) });
    setIsModalVisible(true);
    setIsEditing(false);
  };

  const closeModal = () => {
    setSelectedPlan(null);
    setIsModalVisible(false);
    setIsEditing(false);
  };

  const handleEdit = () => setIsEditing(true);

  // Dlete logic
  const confirmDelete = () => {
    if (!selectedPlan) return;
    setShowDeleteConfirm(true);
  };

  const doDelete = async () => {
    if (!selectedPlan) return;
    try {
      await planService.deletePlan(selectedPlan.id);
      message.success("Plan deleted.");
      setShowDeleteConfirm(false);
      closeModal();
      fetchPlans();
    } catch (err) {
      
      setDeleteError("Cannot delete a plan that is assigned to one or more users");
      message.error("Failed to delete plan.");
    }
  };

  const handleSave = async () => {
    if (!editedPlan) return;

    setErrorMessage("");

    if (!editedPlan.name || editedPlan.name.trim() === "") {
      setErrorMessage("Plan name cannot be empty.");
      return;
    }

    const thresholds = editedPlan.prices.map((p) => p.threshold);
    const prices = editedPlan.prices.map((p) => p.price);

    if (!editedPlan.prices || editedPlan.prices.length === 0) {
      setErrorMessage("Plan must include at least one price threshold.");
      message.error("Plan must include at least one price threshold.");
      return;
    }
    const hasOpenEnded = editedPlan.prices.some((p) => p.threshold === null);
    
    if (!hasOpenEnded) {
      setErrorMessage("Plan must include one open-ended (null) threshold for remaining consumption.");
      message.error("Plan must include one open-ended (null) threshold for remaining consumption.");
      return;
    }
    if (new Set(thresholds).size !== thresholds.length) {
      setErrorMessage("Duplicate thresholds are not allowed.");
      return;
    }
    if (new Set(prices).size !== prices.length) {
      setErrorMessage("Duplicate prices are not allowed.");
      return;
    }

    try {
      await planService.updatePlan(editedPlan.id, editedPlan);
      message.success("Plan updated successfully!");
      closeModal();
      fetchPlans();
    } catch (error) {
      console.error(error);
      message.error("Failed to update plan.");
    }
  };

  // --- ADD PLAN LOGIC ---
  const openAddModal = () => {
    setNewPlan({ name: "", discount: null, prices: [] });
    setErrorMessage("");
    setIsAddModalVisible(true);
  };

  const closeAddModal = () => {
    setIsAddModalVisible(false);
  };

  const handleAddPlan = async () => {
    if (!newPlan.prices || newPlan.prices.length === 0) {
      setErrorMessage("Plan must include at least one price threshold.");
      message.error("Plan must include at least one price threshold.");
      return;
    }

    // ✅ Check that at least one open-ended (null) threshold exists
    const hasOpenEnded = newPlan.prices.some((p) => p.threshold === null);

    if (!hasOpenEnded) {
      setErrorMessage("Plan must include one open-ended (null) threshold for remaining consumption.");
      message.error("Plan must include one open-ended (null) threshold for remaining consumption.");
      return;
    }


    if (!newPlan.name || newPlan.name.trim() === "") {
      setErrorMessage("Plan name cannot be empty.");
      return;
    }

    const thresholds = newPlan.prices.map((p) => p.threshold);
    const prices = newPlan.prices.map((p) => p.price);

    if (new Set(thresholds).size !== thresholds.length) {
      setErrorMessage("Duplicate thresholds are not allowed.");
      return;
    }
    if (new Set(prices).size !== prices.length) {
      setErrorMessage("Duplicate prices are not allowed.");
      return;
    }

    try {
      await planService.createPlan(newPlan);
      message.success("Plan created successfully!");
      closeAddModal();
      fetchPlans();
    } catch (error) {
      console.error(error);
      message.error("Failed to create plan.");
    }
  };

  const columns = [
    { title: "ID", dataIndex: "id", key: "id", width: 80 },
    { title: "Plan Name", dataIndex: "name", key: "name" },
    {
      title: "Discount",
      dataIndex: "discount",
      key: "discount",
      render: (value) => (value ? `${value}%` : "—"),
    },
    {
      title: "Price Thresholds",
      key: "prices",
      render: (_, record) => (
        <>
          {record.prices && record.prices.length > 0 ? (
            record.prices.map((p, index) => (
              <Tag key={index} color="blue">
                {p.threshold === null ? "Remaining kWh" : `${p.threshold} kWh`} → {p.price} €/kWh
              </Tag>
            ))
          ) : (
            <span>—</span>
          )}
        </>
      ),
    },
  ];

  return (
    <Card title="Plan Dashboard" className="w-96 shadow-md text-center">
      {loading ? (
        <div style={{ textAlign: "center", padding: 50 }}>
          <Spin size="large" />
        </div>
      ) : (
        <div style={{ background: "#fff", padding: 24, borderRadius: 8 }}>
          <Table
            dataSource={plans}
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
            style={{ marginBottom: 16 }}
            onClick={openAddModal}
          >
            + Add New Plan
          </Button>
        </div>
      )}

      {/* VIEW / EDIT MODAL */}
      <Modal
        title={isEditing ? "Edit Plan" : selectedPlan?.name || "Plan Details"}
        open={isModalVisible}
        onCancel={closeModal}
        width={700}
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
                <Button danger onClick={confirmDelete}>
                  Delete
                </Button>,
                <Button key="close" onClick={closeModal}>
                  Close
                </Button>,
              ]
        }
      >
        {editedPlan && (
          <>
            <p>
              <b>Name:</b>{" "}
              <Input
                value={editedPlan.name}
                onChange={(e) =>
                  setEditedPlan({ ...editedPlan, name: e.target.value })
                }
                disabled={!isEditing}
              />
            </p>
            <p>
              <b>Discount (%):</b>{" "}
              <InputNumber
                min={0}
                max={100}
                value={editedPlan.discount}
                onChange={(val) =>
                  setEditedPlan({ ...editedPlan, discount: val })
                }
                disabled={!isEditing}
              />
            </p>

            <p>
              <b>Price Thresholds:</b>
            </p>
            {editedPlan.prices.map((p, index) => (
              <Space key={index} style={{ display: "flex", marginBottom: 8 }}>
                <InputNumber
                  min={0}
                  value={p.threshold}
                  onChange={(val) => {
                    const updated = [...editedPlan.prices];
                    updated[index].threshold = val;
                    setEditedPlan({ ...editedPlan, prices: updated });
                  }}
                  disabled={!isEditing}
                />
                <InputNumber
                  min={0.01}
                  step={0.01}
                  value={p.price}
                  onChange={(val) => {
                    const updated = [...editedPlan.prices];
                    updated[index].price = val;
                    setEditedPlan({ ...editedPlan, prices: updated });
                  }}
                  disabled={!isEditing}
                />
                {isEditing && (
                  <Button
                    danger
                    onClick={() => {
                      const updated = editedPlan.prices.filter(
                        (_, i) => i !== index
                      );
                      setEditedPlan({ ...editedPlan, prices: updated });
                    }}
                  >
                    Remove
                  </Button>
                )}
              </Space>
            ))}
            {isEditing && (
              <Button
                type="dashed"
                onClick={() => {
                  setEditedPlan({
                    ...editedPlan,
                    prices: [
                      ...editedPlan.prices,
                      { threshold: 0, price: 0 },
                    ],
                  });
                }}
                block
              >
                + Add Price Threshold
              </Button>
            )}
            {errorMessage && <p style={{ color: "red" }}>{errorMessage}</p>}
          </>
        )}
      </Modal>

      {/* DELETE CONFIRM MODAL */}
      <Modal
        title="Delete Plan"
        open={showDeleteConfirm}
        onOk={doDelete}
        onCancel={() => setShowDeleteConfirm(false)}
        okText="Delete"
        okType="danger"
        cancelText="No"
        centered
      >
        <p>
          Are you sure you want to delete the plan "
          {selectedPlan?.name}"? This action cannot be undone.
          {deleteError && (
          <p style={{ color: "red", marginTop: 8 }}>{deleteError}</p>
        )}
        </p>
      </Modal>

      {/* ADD PLAN MODAL */}
      <Modal
        title="Add New Plan"
        open={isAddModalVisible}
        onCancel={closeAddModal}
        width={700}
        footer={[
          <Button key="cancel" onClick={closeAddModal}>
            Cancel
          </Button>,
          <Button key="create" type="primary" onClick={handleAddPlan}>
            Create Plan
          </Button>,
        ]}
      >
        <p>
          <b>Name:</b>{" "}
          <Input
            value={newPlan.name}
            onChange={(e) =>
              setNewPlan({ ...newPlan, name: e.target.value })
            }
          />
        </p>
        <p>
          <b>Discount (%):</b>{" "}
          <InputNumber
            min={0}
            max={100}
            value={newPlan.discount}
            onChange={(val) => setNewPlan({ ...newPlan, discount: val })}
          />
        </p>

        <p>
          <b>Price Thresholds:</b>
        </p>
        {newPlan.prices.map((p, index) => (
          <Space key={index} style={{ display: "flex", marginBottom: 8 }}>
            <InputNumber
              min={0}
              value={p.threshold}
              onChange={(val) => {
                const updated = [...newPlan.prices];
                updated[index].threshold = val;
                setNewPlan({ ...newPlan, prices: updated });
              }}
              placeholder="Threshold (kWh)"
            />
            <InputNumber
              min={0.01}
              step={0.01}
              value={p.price}
              onChange={(val) => {
                const updated = [...newPlan.prices];
                updated[index].price = val;
                setNewPlan({ ...newPlan, prices: updated });
              }}
              placeholder="Price (€)"
            />
            <Button
              danger
              onClick={() => {
                const updated = newPlan.prices.filter((_, i) => i !== index);
                setNewPlan({ ...newPlan, prices: updated });
              }}
            >
              Remove
            </Button>
          </Space>
        ))}
        <Button
          type="dashed"
          onClick={() => {
            setNewPlan({
              ...newPlan,
              prices: [...newPlan.prices, { threshold: 0, price: 0 }],
            });
          }}
          block
        >
          + Add Price Threshold
        </Button>
        {errorMessage && (
          <p style={{ color: "red", marginTop: 8 }}>{errorMessage}</p>
        )}
      </Modal>
    </Card>
  );
};

export default PlansDashboard;
