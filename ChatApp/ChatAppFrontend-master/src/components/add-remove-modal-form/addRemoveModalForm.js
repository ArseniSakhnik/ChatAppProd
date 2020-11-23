import React, {useEffect, useState} from 'react'
import {Modal, Button} from "react-bootstrap";
import Error from "../error";
import {useChat} from "../ChatProvider/ChatProvider";
import {useRecipients} from "../ChatProvider/chatProviderRecipients";

export default function AddRemoveModalForm({connection, type, handleShow, handleClose, show, userService}) {

    const [username, setUsername] = useState('')
    const [error, setError] = useState('')

    const handleUsernameUpdate = (e) => {
        setUsername(e.target.value)
    }

    const {dialogId} = useChat()

    const {recipients} = useRecipients()

    const addUserToDialog = () => {
        userService.userExists(username.replace(/\s/g, ''))
            .then(async response => {
                if (connection.connectionStarted) {
                    console.log('add user to dialog form: connection started')
                    try {
                        console.log('adding user to dialog: ')
                        if (recipients.indexOf(username.replace(/\s/g, '') != -1)) {
                            await connection.invoke('AddUserToDialog', username, dialogId + 1)
                        } else {
                            setError("Пользователя с таким именем нет в беседе")
                        }
                    } catch (e) {
                        console.log(e)
                    }
                } else {
                    setError("Не удалось соединиться с сервером")
                }
            })
            .catch(() => {
                setError("Пользователя с таким именем не найдено в диалоге")
            })
    }

    const removeUserFromDialog = () => {
        userService.userExists(username.replace(/\s/g, ''))
            .then(async response => {
                if (connection.connectionStarted) {
                    console.log('remove user from dialog form: connection started')
                    try {
                        console.log('removing user from dialog')
                        if (recipients.indexOf(username.replace(/\s/g, '')) != -1) {
                            await connection.invoke('RemoveUserFromDialog', username, dialogId + 1)
                        } else {
                            setError('Пользователя с таким именем нет в беседе')
                        }
                    } catch (e) {
                        console.log(e)
                    }
                }
            })
            .catch(() => {
                setError("Пользователя с таким именем не найдено в диалоге")
            })
    }

    return (
        <Modal show={show} onHide={handleClose}>
            <Modal.Header closeButton>
                <Modal.Title>{type == 'add' ? <div>Add user to dialogue</div> :
                    <div>Remove user from dialogue</div>}
                </Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <input type='text' onChange={handleUsernameUpdate} value={username}/>
                {error.length > 0 ? <Error errorMessage={error}/> : <div></div>}
            </Modal.Body>
            <Modal.Footer>
                {type === 'add' ? <Button variant="secondary" onClick={addUserToDialog}>Add user to dialog</Button> :
                    <Button variant="secondary" onClick={removeUserFromDialog}>Remove user from dialog</Button>}
            </Modal.Footer>
        </Modal>
    )

}